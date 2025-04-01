using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.ServiceBus.Handlers
{
    /// <summary>
    /// RabbitMQ implementation of IServiceBusHandler
    /// </summary>
    public class ServiceBusHandler : IServiceBusHandler, IDisposable
    {
        private readonly RabbitMQConnection _connection;
        private readonly ILogger<ServiceBusHandler> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private bool _disposed;

        /// <summary>
        /// Constructor with RabbitMQ connection and logger
        /// </summary>
        /// <param name="connection">RabbitMQ connection</param>
        /// <param name="logger">Logger</param>
        public ServiceBusHandler(RabbitMQConnection connection, ILogger<ServiceBusHandler> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception,
                            "Error during RabbitMQ operation. Retry attempt {RetryCount}. Waiting {TimeSpan}s before next attempt.",
                            retryCount, timeSpan.TotalSeconds);
                    });
        }

        /// <inheritdoc/>
        public async Task SendMessageAsync<T>(T message, string queueName, CancellationToken cancellationToken = default) where T : class
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));

            await _retryPolicy.ExecuteAsync(async (ct) =>
            {
                try
                {
                    if (!_connection.IsConnected)
                    {
                        await _connection.TryConnectAsync(ct);
                        if (!_connection.IsConnected)
                        {
                            _logger.LogError("Unable to establish RabbitMQ connection for publishing message");
                            return;
                        }
                    }

                    _logger.LogInformation("Sending message to queue {QueueName}", queueName);

                    using var channel = await _connection.CreateChannelAsync();
                    
                    // Declare the queue (creates if it doesn't exist)
                    await channel.QueueDeclareAsync(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                    
                    // Serialize message to JSON
                    var messageJson = JsonSerializer.Serialize(message);
                    var body = Encoding.UTF8.GetBytes(messageJson);
                    
                    // Set message properties
                    var properties = new BasicProperties()
                    {
                        Persistent = true,
                        ContentType = "application/json",
                    };
                    
                    // Publish message
                    await channel.BasicPublishAsync<BasicProperties>(
                        exchange: "",
                        routingKey: queueName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);
                    
                    _logger.LogInformation("Successfully sent message to queue {QueueName}", queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send message to queue {QueueName}", queueName);
                    throw;
                }
            }, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task SubscribeAsync<T>(string queueName, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));
            
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            await _retryPolicy.ExecuteAsync(async (ct) =>
            {
                try
                {
                    if (!_connection.IsConnected)
                    {
                        await _connection.TryConnectAsync(ct);
                        if (!_connection.IsConnected)
                        {
                            _logger.LogError("Unable to establish RabbitMQ connection for subscribing to queue");
                            return;
                        }
                    }

                    _logger.LogInformation("Setting up subscription to queue {QueueName}", queueName);
                    
                    var channel = await _connection.CreateChannelAsync();
                    
                    // Declare the queue (creates if it doesn't exist)
                    await channel.QueueDeclareAsync(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                    
                    // Fair dispatch - don't give more than one message to a worker at a time
                    await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
                    
                    // Create consumer
                    var consumer = new AsyncEventingBasicConsumer(channel);
                    
                    // Create a linked token source that combines our cancellation token with the consumer's
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    
                    // Add consumer event handler
                    consumer.ReceivedAsync += async (sender, ea) =>
                    {
                        // Check if cancellation was requested
                        if (linkedCts.Token.IsCancellationRequested)
                        {
                            _logger.LogInformation("Message processing cancelled for queue {QueueName}", queueName);
                            return;
                        }
                        
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        
                        try
                        {
                            _logger.LogDebug("Processing message from queue {QueueName}", queueName);
                            
                            // Deserialize message
                            var deserializedMessage = JsonSerializer.Deserialize<T>(message);
                            if (deserializedMessage != null)
                            {
                                // Process message with handler, passing the cancellation token
                                await handler(deserializedMessage, linkedCts.Token);
                                
                                // Acknowledge message after successful processing
                                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                                
                                _logger.LogDebug("Successfully processed message from queue {QueueName}", queueName);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to deserialize message from queue {QueueName}: {Message}", queueName, message);
                                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation("Message processing cancelled for queue {QueueName}", queueName);
                            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message from queue {QueueName}", queueName);
                            // Reject message and requeue
                            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                        }
                    };
                    
                    // Start consuming
                    await channel.BasicConsumeAsync(
                        queue: queueName,
                        autoAck: false,
                        consumer: consumer);
                    
                    _logger.LogInformation("Successfully subscribed to queue {QueueName}", queueName);

                    // Keep the channel open until cancellation is requested
                    try
                    {
                        await Task.Delay(Timeout.Infinite, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Subscription to queue {QueueName} was cancelled", queueName);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Subscription setup for queue {QueueName} was cancelled", queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting up subscription to queue {QueueName}", queueName);
                    throw;
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Disposes managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Disposes managed and unmanaged resources
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Nothing to dispose here as _connection is managed by DI
            }

            _disposed = true;
        }
    }
} 