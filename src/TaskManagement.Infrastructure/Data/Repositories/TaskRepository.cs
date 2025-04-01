using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data.Context;

namespace TaskManagement.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Implementation of ITaskRepository using Entity Framework Core
    /// </summary>
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskManagementDbContext _context;

        /// <summary>
        /// Constructor with DbContext dependency
        /// </summary>
        /// <param name="context">Database context</param>
        public TaskRepository(TaskManagementDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TaskItem> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .FindAsync(new object[] { id }, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TaskItem> AddTaskAsync(TaskItem task, CancellationToken cancellationToken = default)
        {
            await _context.Tasks.AddAsync(task, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return task;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateTaskAsync(TaskItem task, CancellationToken cancellationToken = default)
        {
            _context.Tasks.Update(task);
            int result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default)
        {
            var task = await _context.Tasks.FindAsync(new object[] { id }, cancellationToken);
            if (task == null)
            {
                return false;
            }

            _context.Tasks.Remove(task);
            int result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
        {
            _context.Tasks.Remove(task);
            int result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }
    }
} 