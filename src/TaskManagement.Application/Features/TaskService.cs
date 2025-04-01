using Mapster;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Events;
using TaskManagement.Domain.Interfaces;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Features
{
    /// <summary>
    /// Implementation of ITaskService
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IServiceBusHandler _serviceBusHandler;
        private readonly ILogger<TaskService> _logger;

        /// <summary>
        /// Constructor for TaskService
        /// </summary>
        /// <param name="taskRepository">Task repository</param>
        /// <param name="serviceBusHandler">Service bus handler</param>
        /// <param name="logger">Logger</param>
        public TaskService(
            ITaskRepository taskRepository,
            IServiceBusHandler serviceBusHandler,
            ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _serviceBusHandler = serviceBusHandler ?? throw new ArgumentNullException(nameof(serviceBusHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new task with name: {TaskName}", request.Name);
            
            var taskItem = request.Adapt<TaskItem>();
            
            var createdTask = await _taskRepository.AddTaskAsync(taskItem, cancellationToken);
            
            // Publish event
            var taskCreatedEvent = createdTask.Adapt<TaskCreatedEvent>();
            await _serviceBusHandler.SendMessageAsync(taskCreatedEvent, "task-created", cancellationToken);
            
            return createdTask.Adapt<TaskResponse>();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaskResponse>> GetAllTasksAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all tasks");
            
            var tasks = await _taskRepository.GetAllTasksAsync(cancellationToken);
            return tasks.Adapt<IEnumerable<TaskResponse>>();
        }

        /// <inheritdoc/>
        public async Task<TaskResponse> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting task with id: {TaskId}", id);
            
            var taskItem = await _taskRepository.GetTaskByIdAsync(id, cancellationToken);
            if (taskItem == null)
            {
                _logger.LogWarning("Task with id {TaskId} not found", id);
                return null;
            }
            
            return taskItem.Adapt<TaskResponse>();
        }

        /// <inheritdoc/>
        public async Task<TaskResponse> AssignTaskAsync(int id, AssignTaskRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Assigning task {TaskId} to {Assignee}", id, request.Assignee);
            
            var taskItem = await _taskRepository.GetTaskByIdAsync(id, cancellationToken);
            if (taskItem == null)
            {
                _logger.LogWarning("Task with id {TaskId} not found", id);
                return null;
            }
            
            taskItem.AssignedTo = request.Assignee;
            
            await _taskRepository.UpdateTaskAsync(taskItem, cancellationToken);
            
            // Publish event
            var taskAssignedEvent = taskItem.Adapt<TaskAssignedEvent>();
            taskAssignedEvent.AssigneeName = request.Assignee;
            await _serviceBusHandler.SendMessageAsync(taskAssignedEvent, "task-assigned", cancellationToken);
            
            return taskItem.Adapt<TaskResponse>();
        }

        /// <inheritdoc/>
        public async Task<TaskResponse> UpdateTaskStatusAsync(int id, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating task {TaskId} status to {Status}", id, request.NewStatus);
            
            var validValues = Enum.GetValues(typeof(TaskStatus)).Cast<TaskStatus>();
            if (!validValues.Contains(request.NewStatus))
            {
                _logger.LogWarning("Invalid status {Status} provided for task {TaskId}", request.NewStatus, id);
                throw new ArgumentException($"Invalid task status: {request.NewStatus}. Valid values are: NotStarted (0), InProgress (1), Completed (2).");
            }
            
            var taskItem = await _taskRepository.GetTaskByIdAsync(id, cancellationToken);
            if (taskItem == null)
            {
                _logger.LogWarning("Task with id {TaskId} not found", id);
                return null;
            }
            
            taskItem.Status = request.NewStatus;
            
            await _taskRepository.UpdateTaskAsync(taskItem, cancellationToken);
            
            // Publish event
            var taskUpdatedEvent = taskItem.Adapt<TaskUpdatedEvent>();
            taskUpdatedEvent.Status = request.NewStatus.ToString();
            await _serviceBusHandler.SendMessageAsync(taskUpdatedEvent, "task-updated", cancellationToken);
            
            return taskItem.Adapt<TaskResponse>();
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting task with id: {TaskId}", id);
            
            var taskItem = await _taskRepository.GetTaskByIdAsync(id, cancellationToken);
            if (taskItem == null)
            {
                _logger.LogWarning("Task with id {TaskId} not found", id);
                return false;
            }
            
            return await _taskRepository.DeleteTaskAsync(id, cancellationToken);
        }
    }
} 