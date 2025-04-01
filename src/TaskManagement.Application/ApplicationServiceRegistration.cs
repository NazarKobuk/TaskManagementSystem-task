using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaskManagement.Application.Features;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Mappings;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application
{
    /// <summary>
    /// Contains extension methods for registering application services
    /// </summary>
    public static class ApplicationServiceRegistration
    {
        /// <summary>
        /// Adds application services to the DI container
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>The same service collection</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register Mapster
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly());
            
            // Register the mapping configurations
            config.Apply(new MappingRegister());
            
            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();

            // Register FluentValidation
            services.AddFluentValidation(config =>
            {
                config.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()); // - fix it later to use not deprecated version of the method
            });

            // Register services
            services.AddScoped<ITaskService, TaskService>();

            return services;
        }
    }
} 