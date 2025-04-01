using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManagement.Api.Controllers;
using System.Collections.Generic;
using System.Text.Json;
using TaskManagement.Tests.Helpers;

namespace TaskManagement.Api.Tests.Controllers
{
    [TestFixture]
    public class ErrorControllerTests
    {
        private Mock<ILogger<ErrorController>> _mockLogger = null!;
        private Mock<IWebHostEnvironment> _mockEnvironment = null!;
        private ErrorController _controller = null!;
        private DefaultHttpContext _httpContext = null!;
        private Mock<IExceptionHandlerFeature> _mockExceptionFeature = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ErrorController>>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);
            
            _httpContext = new DefaultHttpContext();
            _mockExceptionFeature = new Mock<IExceptionHandlerFeature>();
            
            _controller = new ErrorController(_mockLogger.Object, _mockEnvironment.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        [Test]
        public void Error_WithException_ReturnsErrorDetails()
        {
            // Arrange
            var exceptionMessage = "Test exception message";
            var exception = new Exception(exceptionMessage);
            
            _mockExceptionFeature.Setup(e => e.Error).Returns(exception);
            _httpContext.Features.Set(_mockExceptionFeature.Object);

            // Setup logger
            _mockLogger.SetupLog(LogLevel.Error, exception, "An unhandled exception occurred.");

            // Act
            var result = _controller.Error();

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            
            var errorResponse = objectResult.Value;
            Assert.IsNotNull(errorResponse);
            
            // Access properties using JsonElement cast and GetProperty
            var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(errorResponse));
                
            Assert.IsTrue(responseDict.ContainsKey("Message"));
            Assert.AreEqual("An error occurred while processing your request.", responseDict["Message"].ToString());
            
            Assert.IsTrue(responseDict.ContainsKey("Error"));
            Assert.IsNotNull(responseDict["Error"]);
            
            var errorDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(responseDict["Error"]));
                
            Assert.IsTrue(errorDict.ContainsKey("ExceptionMessage"));
            Assert.AreEqual(exceptionMessage, errorDict["ExceptionMessage"].ToString());
            
            // Verify logger was called with appropriate error information
            _mockLogger.VerifyLog(LogLevel.Error, exception, "An unhandled exception occurred.", Times.Once());
        }

        [Test]
        public void Error_WithoutException_ReturnsErrorDetails()
        {
            // Arrange
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.Error();

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = (ObjectResult)result;

            // Check status code
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            // Verify response contains error details
            var responseJson = JsonSerializer.Serialize(objectResult.Value);
            var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);

            Assert.IsTrue(responseDictionary!.ContainsKey("Message"));
            Assert.IsTrue(responseDictionary.ContainsKey("Error"));
            
            // No logger verification - when there's no exception, the logger isn't called
        }

        [Test]
        public void Error_InNonDevelopmentEnvironment_HidesExceptionDetails()
        {
            // Arrange
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
            
            var exceptionMessage = "Test exception message";
            var exception = new Exception(exceptionMessage);
            
            _mockExceptionFeature.Setup(e => e.Error).Returns(exception);
            _httpContext.Features.Set(_mockExceptionFeature.Object);

            // Setup logger
            _mockLogger.SetupLog(LogLevel.Error, exception, "An unhandled exception occurred.");

            // Act
            var result = _controller.Error();

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            
            var errorResponse = objectResult.Value;
            Assert.IsNotNull(errorResponse);
            
            // Access properties using JsonElement cast and GetProperty
            var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(errorResponse));
                
            Assert.IsTrue(responseDict.ContainsKey("Message"));
            Assert.AreEqual("An error occurred while processing your request.", responseDict["Message"].ToString());
            
            Assert.IsTrue(responseDict.ContainsKey("Error"));
            Assert.IsNull(responseDict["Error"]);
            
            // Verify logger was called with error information but without exposing details
            _mockLogger.VerifyLog(LogLevel.Error, exception, "An unhandled exception occurred.", Times.Once());
        }
    }
} 