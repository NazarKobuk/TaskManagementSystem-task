using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TaskManagement.Api.Filters;
using TaskManagement.Api.Middleware;
using TaskManagement.Application;
using TaskManagement.Application.Features;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Mappings;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Context;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.ServiceBus;
using TaskManagement.ServiceBus.Handlers;

namespace TaskManagement.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Create and migrate the database
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                
                try
                {
                    // Get the DbContext instance
                    var dbContext = services.GetRequiredService<TaskManagementDbContext>();
                    
                    // Apply migrations
                    logger.LogInformation("Applying database migrations...");
                    dbContext.Database.Migrate();
                    logger.LogInformation("Database migrations applied successfully.");
                    
                    // Seed initial data
                    logger.LogInformation("Seeding database with initial data...");
                    SeedData.Initialize(services);
                    logger.LogInformation("Database seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add controllers with filters
            services.AddControllers(options =>
            {
                options.Filters.Add<ApiExceptionFilter>();
            });

            // Register application services including validators
            services.AddApplicationServices();

            // Add CORS policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Register DB Context
            services.AddDbContext<TaskManagementDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => {
                        sqlOptions.MigrationsAssembly("TaskManagement.Infrastructure");
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

            // Register repositories
            services.AddScoped<ITaskRepository, TaskRepository>();

            // Register services
            services.AddScoped<ITaskService, TaskService>();

            // Register Mapster configuration
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            new MappingRegister().Register(typeAdapterConfig);

            // Register ServiceBus handler (with resilience for development)
            if (_env.IsDevelopment())
            {
                try
                {
                    // Register ServiceBus handler
                    services.AddScoped<IServiceBusHandler, ServiceBusHandler>();
                    // Register ServiceBus services
                    services.AddServiceBusServices(Configuration);
                }
                catch (Exception ex)
                {
                    var logger = services.BuildServiceProvider().GetRequiredService<ILogger<Startup>>();
                    logger.LogWarning(ex, "Unable to register RabbitMQ services. Application will run without messaging capabilities.");
                    
                    // Add dummy service bus handler for development
                    services.AddScoped<IServiceBusHandler, DummyServiceBusHandler>();
                }
            }
            else
            {
                // Register ServiceBus services
                try
                {
                    services.AddServiceBusServices(Configuration);
                    var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
                    var serviceLogger = loggerFactory.CreateLogger<Startup>();
                    serviceLogger.LogInformation("RabbitMQ service bus registered successfully");
                }
                catch (Exception ex)
                {
                    // If in development environment, we can continue with a dummy handler
                    if (_env.IsDevelopment())
                    {
                        var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
                        var serviceLogger = loggerFactory.CreateLogger<Startup>();
                        serviceLogger.LogWarning(ex, "Failed to register RabbitMQ services. Using dummy service bus handler instead.");
                        services.AddSingleton<IServiceBusHandler, DummyServiceBusHandler>();
                    }
                    else
                    {
                        // In production, we want to know if RabbitMQ setup fails
                        var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
                        var serviceLogger = loggerFactory.CreateLogger<Startup>();
                        serviceLogger.LogError(ex, "Failed to register RabbitMQ services.");
                        throw;
                    }
                }
            }

            // Configure Swagger
            ConfigureSwagger(services);
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Task Management API",
                    Version = "v1",
                    Description = "API for managing tasks with service bus integration",
                    Contact = new OpenApiContact
                    {
                        Name = "Task Management Team",
                        Email = "support@taskmanagement.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                // Include XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                // Use annotations
                options.EnableAnnotations();

                // Add tag descriptions
                options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
                options.DocInclusionPredicate((docName, apiDesc) => true);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
                    options.RoutePrefix = string.Empty;  // Set Swagger as the default landing page
                    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                    options.DefaultModelsExpandDepth(0); // Hide models section by default
                    options.DisplayRequestDuration();    // Show request duration
                });
            }
            else
            {
                // Add production exception handling
                app.UseExceptionHandler("/error");
                // Enable HTTP Strict Transport Security
                app.UseHsts();
                
                // Add Swagger for non-dev environments with customized options
                app.UseSwagger(options =>
                {
                    options.RouteTemplate = "api-docs/{documentName}/swagger.json";
                });
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/api-docs/v1/swagger.json", "Task Management API v1");
                    options.RoutePrefix = "api-docs";
                });
            }

            // Add custom request logging middleware
            app.UseRequestLogging();

            app.UseHttpsRedirection();

            app.UseRouting();

            // Add CORS middleware
            app.UseCors("AllowAll");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    
    // Fallback implementation of IServiceBusHandler for development environments
    public class DummyServiceBusHandler : IServiceBusHandler
    {
        private readonly ILogger<DummyServiceBusHandler> _logger;
        
        public DummyServiceBusHandler(ILogger<DummyServiceBusHandler> logger)
        {
            _logger = logger;
        }
        
        public Task SendMessageAsync<T>(T message, string queueName, CancellationToken cancellationToken = default) where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            _logger.LogWarning("DummyServiceBusHandler: Message to {QueueName} was not sent because RabbitMQ is not available. Message: {MessageType}", 
                queueName, typeof(T).Name);
            return Task.CompletedTask;
        }
        
        public Task SubscribeAsync<T>(string queueName, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            _logger.LogWarning("DummyServiceBusHandler: Subscription to {QueueName} was ignored because RabbitMQ is not available. MessageType: {MessageType}", 
                queueName, typeof(T).Name);
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
