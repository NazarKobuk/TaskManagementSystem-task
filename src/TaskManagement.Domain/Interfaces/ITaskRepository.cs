using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for task operations
    /// </summary>
    public interface ITaskRepository
    {
        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Collection of all tasks</returns>
        Task<IEnumerable<TaskItem>> GetAllTasksAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a task by its id
        /// </summary>
        /// <param name="id">Id of the task to retrieve</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The task if found, otherwise null</returns>
        Task<TaskItem> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new task
        /// </summary>
        /// <param name="task">The task to add</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The added task with generated Id</returns>
        Task<TaskItem> AddTaskAsync(TaskItem task, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing task
        /// </summary>
        /// <param name="task">The task with updated values</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateTaskAsync(TaskItem task, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a task by its id
        /// </summary>
        /// <param name="id">Id of the task to delete</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default);
    }
} 