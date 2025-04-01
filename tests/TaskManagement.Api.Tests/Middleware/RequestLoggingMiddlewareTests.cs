using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManagement.Api.Middleware;
using TaskManagement.Tests.Helpers;

namespace TaskManagement.Api.Tests.Middleware
{
    [TestFixture]
    public class RequestLoggingMiddlewareTests
    {
        private RequestLoggingMiddleware _middleware = null!;
        private Mock<ILogger<RequestLoggingMiddleware>> _mockLogger = null!;
        private Mock<RequestDelegate> _mockNextMiddleware = null!;
        private HttpContext _httpContext = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<RequestLoggingMiddleware>>();
            _mockNextMiddleware = new Mock<RequestDelegate>();
            _middleware = new RequestLoggingMiddleware(_mockNextMiddleware.Object, _mockLogger.Object);

            // Setup HTTP context
            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Method = "GET";
            _httpContext.Request.Path = "/api/task";
            _httpContext.Response.Body = new MemoryStream();
        }

        [Test]
        public async Task InvokeAsync_LogsRequestAndResponse()
        {
            // Arrange
            _mockNextMiddleware
                .Setup(n => n(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask)
                .Callback(() => _httpContext.Response.StatusCode = StatusCodes.Status200OK);

            // Setup logger expectations
            var requestRegex = new Regex("Request started");
            var responseRegex = new Regex("Request finished");

            _mockLogger.SetupLog(LogLevel.Information, requestRegex);
            _mockLogger.SetupLog(LogLevel.Information, responseRegex);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert - Verify the logger was called for both request and response
            _mockLogger.VerifyLog(LogLevel.Information, requestRegex, Times.Once());
            _mockLogger.VerifyLog(LogLevel.Information, responseRegex, Times.Once());
        }

        [Test]
        public async Task InvokeAsync_LogsErrorStatusCode()
        {
            // Arrange
            _mockNextMiddleware
                .Setup(n => n(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask)
                .Callback(() => _httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError);

            // Setup logger expectations
            var requestRegex = new Regex("Request started");
            var statusRegex = new Regex("Status: 500");

            _mockLogger.SetupLog(LogLevel.Information, requestRegex);
            _mockLogger.SetupLog(LogLevel.Information, statusRegex);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert - Verify the logger was called for both request and response with error status
            _mockLogger.VerifyLog(LogLevel.Information, requestRegex, Times.Once());
            _mockLogger.VerifyLog(LogLevel.Information, statusRegex, Times.Once());
        }

        [Test]
        public async Task InvokeAsync_HandlesExceptionInNextMiddleware()
        {
            // Arrange
            var testException = new Exception("Test exception");
            
            _mockNextMiddleware
                .Setup(n => n(It.IsAny<HttpContext>()))
                .ThrowsAsync(testException);

            // Setup logger expectations
            var requestRegex = new Regex("Request started");
            var errorRegex = new Regex("Request failed");

            _mockLogger.SetupLog(LogLevel.Information, requestRegex);
            _mockLogger.SetupLog(LogLevel.Error, (Exception ex) => ex.Message == testException.Message, errorRegex);

            // Act & Assert
            try
            {
                await _middleware.InvokeAsync(_httpContext);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                // Verify exception is from the next middleware
                Assert.That(ex.Message, Is.EqualTo("Test exception"));
                
                // Verify the logger was called for request and error
                _mockLogger.VerifyLog(LogLevel.Information, requestRegex, Times.Once());
                _mockLogger.VerifyLog(LogLevel.Error, (Exception exception) => exception.Message == testException.Message, errorRegex, Times.Once());
            }
        }
    }
} 