using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Events;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.ServiceBus.Consumers
{
    /// <summary>
    /// Background service that consumes TaskUpdated events
    /// </summary>
    public class TaskUpdatedConsumer : BackgroundService
    {
        private readonly IServiceBusHandler _serviceBusHandler;
        private readonly ILogger<TaskUpdatedConsumer> _logger;

        /// <summary>
        /// Constructor with service bus handler and logger
        /// </summary>
        /// <param name="serviceBusHandler">Service bus handler</param>
        /// <param name="logger">Logger</param>
        public TaskUpdatedConsumer(
            IServiceBusHandler serviceBusHandler,
            ILogger<TaskUpdatedConsumer> logger)
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
            _logger.LogInformation("Starting TaskUpdatedConsumer");

            try
            {
                await _serviceBusHandler.SubscribeAsync<TaskUpdatedEvent>(
                    ServiceBusQueues.TaskUpdated,
                    async (message, token) => await ProcessMessageAsync(message, token),
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("TaskUpdatedConsumer was canceled");
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error in TaskUpdatedConsumer");
                
                // Wait before retry
                await Task.Delay(5000, stoppingToken);
                
                // Retrying by throwing to have the background service restart
                throw;
            }
        }

        private async Task ProcessMessageAsync(TaskUpdatedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing TaskUpdatedEvent: Task {TaskId} - {TaskName} status updated to {NewStatus} at {UpdatedAt}",
                message.Id, message.TaskName, message.Status, message.UpdatedAt);
            
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();
            
            await Task.CompletedTask;
        }
    }
} 