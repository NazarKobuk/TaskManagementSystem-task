using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using TaskManagement.ServiceBus.Configuration;

namespace TaskManagement.ServiceBus.Handlers
{
    /// <summary>
    /// Manages RabbitMQ connection with retry logic
    /// </summary>
    public class RabbitMQConnection : IDisposable
    {
        private readonly RabbitMQSettings _settings;
        private readonly ILogger<RabbitMQConnection> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private IConnection _connection;
        private bool _disposed;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

        public RabbitMQConnection(
            IOptions<RabbitMQSettings> settings,
            ILogger<RabbitMQConnection> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    _settings.MaxRetryAttempts,
                    retryAttempt => TimeSpan.FromMilliseconds(_settings.RetryIntervalMs * Math.Pow(2, retryAttempt - 1)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception,
                            "Failed to connect to RabbitMQ. Retry attempt {RetryCount}. Waiting {TimeSpan}ms before next attempt.",
                            retryCount, timeSpan.TotalMilliseconds);
                    });
        }

        /// <summary>
        /// Gets whether the connection is currently established.
        /// </summary>
        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        /// <summary>
        /// Creates a new RabbitMQ channel.
        /// </summary>
        /// <returns>A new channel instance.</returns>
        public async Task<IChannel> CreateChannelAsync()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connection is available");
            }

            return await _connection.CreateChannelAsync();
        }

        /// <summary>
        /// Tries to establish a connection to RabbitMQ with retry logic.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if connection is successful; otherwise, false.</returns>
        public async Task<bool> TryConnectAsync(CancellationToken cancellationToken = default)
        {
            if (IsConnected)
            {
                return true;
            }

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (IsConnected)
                {
                    return true;
                }

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _settings.HostName,
                        Port = _settings.Port,
                        UserName = _settings.UserName,
                        Password = _settings.Password,
                        VirtualHost = _settings.VirtualHost
                    };

                    _connection = await factory.CreateConnectionAsync(cancellationToken);
                    _logger.LogInformation("RabbitMQ connection established to {HostName}:{Port}", _settings.HostName, _settings.Port);
                });

                return IsConnected;
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogCritical(ex, "RabbitMQ broker unreachable at {HostName}:{Port}", _settings.HostName, _settings.Port);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Could not establish connection to RabbitMQ after retries");
                return false;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Disposes of the RabbitMQ connection.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error during disposal of RabbitMQConnection");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
} 