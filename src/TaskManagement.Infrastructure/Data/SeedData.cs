using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data.Context;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Data
{
    /// <summary>
    /// Provides methods for seeding the database with initial data
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Initialize the database with seed data
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TaskManagementDbContext>>();

            try
            {
                var context = services.GetRequiredService<TaskManagementDbContext>();
                
                // Seed the database
                SeedTasks(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private static void SeedTasks(TaskManagementDbContext context, ILogger logger)
        {
            // Only seed if no tasks exist
            if (context.Tasks.Any())
            {
                logger.LogInformation("Skipping task seeding as tasks already exist in the database");
                return;
            }

            logger.LogInformation("Adding sample tasks to the database");
            
            // Add sample tasks
            context.Tasks.AddRange(
                new TaskItem
                {
                    Name = "Implement user authentication",
                    Description = "Add user registration and login functionality using JWT",
                    Status = TaskStatus.NotStarted
                },
                new TaskItem
                {
                    Name = "Create API documentation",
                    Description = "Document all API endpoints using Swagger",
                    Status = TaskStatus.Completed
                },
                new TaskItem
                {
                    Name = "Implement message queuing",
                    Description = "Set up RabbitMQ for asynchronous processing of events",
                    Status = TaskStatus.InProgress,
                    AssignedTo = "john.doe@example.com"
                },
                new TaskItem
                {
                    Name = "Design database schema",
                    Description = "Create the initial database schema for the application",
                    Status = TaskStatus.Completed
                },
                new TaskItem
                {
                    Name = "Implement frontend UI",
                    Description = "Create React components for the task management interface",
                    Status = TaskStatus.NotStarted,
                    AssignedTo = "jane.smith@example.com"
                }
            );

            context.SaveChanges();
            logger.LogInformation("Added 5 sample tasks to the database");
        }
    }
} 