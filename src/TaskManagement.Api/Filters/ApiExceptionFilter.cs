using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskManagement.Api.Filters
{
    /// <summary>
    /// Global exception filter to handle and log API exceptions
    /// </summary>
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        /// <summary>
        /// Constructor for exception filter
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handle exception occurrence
        /// </summary>
        /// <param name="context">Exception context</param>
        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception occurred while executing request {Path}",
                context.HttpContext.Request.Path);

            var statusCode = StatusCodes.Status500InternalServerError;
            var title = "An error occurred while processing your request.";

            // Handle specific exception types
            var exceptionType = context.Exception.GetType();
            if (exceptionType == typeof(ArgumentException) || exceptionType == typeof(ArgumentNullException))
            {
                statusCode = StatusCodes.Status400BadRequest;
                title = "Invalid request parameters.";
            }
            else if (exceptionType == typeof(KeyNotFoundException))
            {
                statusCode = StatusCodes.Status404NotFound;
                title = "Resource not found.";
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = context.Exception.Message,
                Instance = context.HttpContext.Request.Path
            };

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
        }
    }
} 