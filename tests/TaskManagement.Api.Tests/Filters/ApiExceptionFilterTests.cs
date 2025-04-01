using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManagement.Api.Filters;

namespace TaskManagement.Api.Tests.Filters
{
    [TestFixture]
    public class ApiExceptionFilterTests
    {
        private Mock<ILogger<ApiExceptionFilter>> _mockLogger = null!;
        private ApiExceptionFilter _filter = null!;
        private ExceptionContext _context = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ApiExceptionFilter>>();
            _filter = new ApiExceptionFilter(_mockLogger.Object);
            
            // Setup ExceptionContext
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };
            
            _context = new ExceptionContext(actionContext, new List<IFilterMetadata>());
        }

        [Test]
        public void OnException_WithStandardException_ReturnsInternalServerError()
        {
            // Arrange
            var exception = new Exception("Test exception");
            _context.Exception = exception;

            // Act
            _filter.OnException(_context);

            // Assert
            Assert.IsTrue(_context.ExceptionHandled);
            Assert.IsInstanceOf<ObjectResult>(_context.Result);
            
            var result = _context.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result!.StatusCode);
            
            var problemDetails = result.Value as ProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual("An error occurred while processing your request.", problemDetails!.Title);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, problemDetails.Status);
            Assert.AreEqual(exception.Message, problemDetails.Detail);
        }

        [Test]
        public void OnException_WithArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var exception = new ArgumentException("Invalid argument");
            _context.Exception = exception;

            // Act
            _filter.OnException(_context);

            // Assert
            Assert.IsTrue(_context.ExceptionHandled);
            Assert.IsInstanceOf<ObjectResult>(_context.Result);
            
            var result = _context.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result!.StatusCode);
            
            var problemDetails = result.Value as ProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual("Invalid request parameters.", problemDetails!.Title);
            Assert.AreEqual(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.AreEqual(exception.Message, problemDetails.Detail);
        }

        [Test]
        public void OnException_WithArgumentNullException_ReturnsBadRequest()
        {
            // Arrange
            var exception = new ArgumentNullException("parameter", "Required parameter is null");
            _context.Exception = exception;

            // Act
            _filter.OnException(_context);

            // Assert
            Assert.IsTrue(_context.ExceptionHandled);
            Assert.IsInstanceOf<ObjectResult>(_context.Result);
            
            var result = _context.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result!.StatusCode);
            
            var problemDetails = result.Value as ProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual("Invalid request parameters.", problemDetails!.Title);
            Assert.AreEqual(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.IsTrue(problemDetails.Detail!.Contains("Required parameter is null"));
        }

        [Test]
        public void OnException_WithKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var exception = new KeyNotFoundException("Item not found");
            _context.Exception = exception;

            // Act
            _filter.OnException(_context);

            // Assert
            Assert.IsTrue(_context.ExceptionHandled);
            Assert.IsInstanceOf<ObjectResult>(_context.Result);
            
            var result = _context.Result as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result!.StatusCode);
            
            var problemDetails = result.Value as ProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual("Resource not found.", problemDetails!.Title);
            Assert.AreEqual(StatusCodes.Status404NotFound, problemDetails.Status);
            Assert.AreEqual(exception.Message, problemDetails.Detail);
        }
    }
} 