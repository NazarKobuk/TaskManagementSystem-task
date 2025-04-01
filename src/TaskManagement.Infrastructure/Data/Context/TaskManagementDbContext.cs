using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.Context
{
    /// <summary>
    /// Database context for Task Management System
    /// </summary>
    public class TaskManagementDbContext : DbContext
    {
        /// <summary>
        /// Constructor with DbContextOptions
        /// </summary>
        /// <param name="options">Database context options</param>
        public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Collection of tasks in the database
        /// </summary>
        public DbSet<TaskItem> Tasks { get; set; }

        /// <summary>
        /// Configure the model
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskManagementDbContext).Assembly);
        }
    }
} 