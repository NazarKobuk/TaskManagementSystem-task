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
    /// Background service that consumes TaskAssigned events
    /// </summary>
    public class TaskAssignedConsumer : BackgroundService
    {
        private readonly IServiceBusHandler _serviceBusHandler;
        private readonly ILogger<TaskAssignedConsumer> _logger;

        /// <summary>
        /// Constructor with service bus handler and logger
        /// </summary>
        /// <param name="serviceBusHandler">Service bus handler</param>
        /// <param name="logger">Logger</param>
        public TaskAssignedConsumer(
            IServiceBusHandler serviceBusHandler,
            ILogger<TaskAssignedConsumer> logger)
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
            _logger.LogInformation("Starting TaskAssignedConsumer");

            try
            {
                await _serviceBusHandler.SubscribeAsync<TaskAssignedEvent>(
                    ServiceBusQueues.TaskAssigned,
                    async (message, token) => await ProcessMessageAsync(message, token),
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("TaskAssignedConsumer was canceled");
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error in TaskAssignedConsumer");
                
                // Wait before retry
                await Task.Delay(5000, stoppingToken);
                
                // Retrying by throwing to have the background service restart
                throw;
            }
        }

        private async Task ProcessMessageAsync(TaskAssignedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing TaskAssignedEvent: Task {TaskId} - {TaskName} assigned to {Assignee} at {AssignedAt}",
                message.Id, message.TaskName, message.AssigneeName, message.AssignedAt);

            // Here you would implement the actual business logic for processing task assignment events
            // For example, sending notifications to the assignee, updating user dashboards, etc.
            
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();
            
            await Task.CompletedTask;
        }
    }
} 