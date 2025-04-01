using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;

namespace TaskManagement.Application.Interfaces
{
    /// <summary>
    /// Service interface for task operations
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of all tasks</returns>
        Task<IEnumerable<TaskResponse>> GetAllTasksAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a task by its id
        /// </summary>
        /// <param name="id">Id of the task to retrieve</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The task if found, otherwise null</returns>
        Task<TaskResponse> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new task
        /// </summary>
        /// <param name="request">Task creation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created task</returns>
        Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the status of a task
        /// </summary>
        /// <param name="id">Id of the task to update</param>
        /// <param name="request">Status update request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated task if successful, null otherwise</returns>
        Task<TaskResponse> UpdateTaskStatusAsync(int id, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Assigns a task to a user
        /// </summary>
        /// <param name="id">Id of the task to assign</param>
        /// <param name="request">Assignment request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated task if successful, null otherwise</returns>
        Task<TaskResponse> AssignTaskAsync(int id, AssignTaskRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a task
        /// </summary>
        /// <param name="id">Id of the task to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default);
    }
} 