using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManagement.Api.Middleware;

namespace TaskManagement.Api.Tests.Middleware
{
    [TestFixture]
    public class MiddlewareExtensionsTests
    {
        private Mock<IApplicationBuilder> _mockAppBuilder;
        private Mock<ILogger<RequestLoggingMiddleware>> _mockLogger;
        private IServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            _mockAppBuilder = new Mock<IApplicationBuilder>();
            _mockLogger = new Mock<ILogger<RequestLoggingMiddleware>>();

            // Create services with the logger
            var services = new ServiceCollection();
            services.AddSingleton(_mockLogger.Object);
            _serviceProvider = services.BuildServiceProvider();

            // Setup the application builder to return itself for method chaining
            _mockAppBuilder
                .Setup(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
                .Returns(_mockAppBuilder.Object);

            // Setup the service provider
            _mockAppBuilder
                .Setup(x => x.ApplicationServices)
                .Returns(_serviceProvider);
        }

        [Test]
        public void UseRequestLogging_AddsMiddleware()
        {
            // Act
            var result = MiddlewareExtensions.UseRequestLogging(_mockAppBuilder.Object);

            // Assert
            // Verify middleware was added
            _mockAppBuilder.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
            
            // Verify it returns the same builder for chaining
            Assert.That(result, Is.SameAs(_mockAppBuilder.Object));
        }
    }
} 