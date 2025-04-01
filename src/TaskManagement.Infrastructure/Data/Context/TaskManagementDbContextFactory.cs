using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace TaskManagement.Infrastructure.Data.Context
{
    /// <summary>
    /// Factory for creating a database context during design time
    /// Used by Entity Framework Core tools for migrations
    /// </summary>
    public class TaskManagementDbContextFactory : IDesignTimeDbContextFactory<TaskManagementDbContext>
    {
        /// <summary>
        /// Creates a new instance of TaskManagementDbContext
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>An instance of TaskManagementDbContext</returns>
        public TaskManagementDbContext CreateDbContext(string[] args)
        {
            // Get the directory where the application assembly resides
            var basePath = Directory.GetCurrentDirectory();
            
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<TaskManagementDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new TaskManagementDbContext(optionsBuilder.Options);
        }
    }
} 