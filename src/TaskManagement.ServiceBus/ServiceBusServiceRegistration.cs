using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using TaskManagement.Domain.Interfaces;
using TaskManagement.ServiceBus.Configuration;
using TaskManagement.ServiceBus.Consumers;
using TaskManagement.ServiceBus.Handlers;

namespace TaskManagement.ServiceBus
{
    /// <summary>
    /// Contains extension methods for registering service bus services
    /// </summary>
    public static class ServiceBusServiceRegistration
    {
        /// <summary>
        /// Adds service bus services to the DI container
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>The same service collection</returns>
        public static IServiceCollection AddServiceBusServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure RabbitMQ settings
            services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQSettings"));
            
            // Add RabbitMQ connection
            services.AddSingleton<RabbitMQConnection>();
            
            // Add RabbitMQ service bus handler
            services.AddSingleton<IServiceBusHandler, ServiceBusHandler>(sp =>
            {
                var connection = sp.GetRequiredService<RabbitMQConnection>();
                var logger = sp.GetRequiredService<ILogger<ServiceBusHandler>>();
                
                try
                {
                    var handler = new ServiceBusHandler(connection, logger);
                    return handler;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create ServiceBusHandler");
                    throw;
                }
            });
            
            // Register consumers as hosted services
            services.AddHostedService<TaskCreatedConsumer>();
            services.AddHostedService<TaskUpdatedConsumer>();
            services.AddHostedService<TaskAssignedConsumer>();
            
            return services;
        }
    }
} 