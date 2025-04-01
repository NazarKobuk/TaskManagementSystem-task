using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Events;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.ServiceBus.Consumers
{
    /// <summary>
    /// Background service that consumes TaskCreated events
    /// </summary>
    public class TaskCreatedConsumer : BackgroundService
    {
        private readonly IServiceBusHandler _serviceBusHandler;
        private readonly ILogger<TaskCreatedConsumer> _logger;

        /// <summary>
        /// Constructor with service bus handler and logger
        /// </summary>
        /// <param name="serviceBusHandler">Service bus handler</param>
        /// <param name="logger">Logger</param>
        public TaskCreatedConsumer(
            IServiceBusHandler serviceBusHandler,
            ILogger<TaskCreatedConsumer> logger)
        {
            _serviceBusHandler = serviceBusHandler ?? throw new ArgumentNullException(nameof(serviceBusHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Start the consumer as a background service
        /// </summary>
        /// <param name="stoppingToken">Cancellation token</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting TaskCreatedConsumer");

            try
            {
                await _serviceBusHandler.SubscribeAsync<TaskCreatedEvent>(
                    ServiceBusQueues.TaskCreated,
                    async (message, token) => await ProcessMessageAsync(message, token),
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("TaskCreatedConsumer was canceled");
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error in TaskCreatedConsumer");
                
                // Wait before retry
                await Task.Delay(5000, stoppingToken);
                
                // Retrying by throwing to have the background service restart
                throw;
            }
        }

        private async Task ProcessMessageAsync(TaskCreatedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing TaskCreatedEvent: Task {TaskId} - {TaskName} was created at {CreatedAt}",
                message.Id, message.TaskName, message.CreatedAt);

            // Here you would implement the actual business logic for processing task creation events
            // For example, sending notifications, updating analytics, etc.
            
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();
            
            await Task.CompletedTask;
        }
    }
} 