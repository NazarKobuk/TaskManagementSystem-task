using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data.Context;
using TaskManagement.Infrastructure.Data.Repositories;

namespace TaskManagement.Infrastructure
{
    /// <summary>
    /// Contains extension methods for registering infrastructure services
    /// </summary>
    public static class InfrastructureServiceRegistration
    {
        /// <summary>
        /// Adds infrastructure services to the DI container
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>The same service collection</returns>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register database context
            services.AddDbContext<TaskManagementDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(TaskManagementDbContext).Assembly.FullName)));

            // Register repositories
            services.AddScoped<ITaskRepository, TaskRepository>();

            return services;
        }
    }
} 