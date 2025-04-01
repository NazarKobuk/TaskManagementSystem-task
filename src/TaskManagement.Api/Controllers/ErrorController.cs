using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.Api.Controllers
{
    /// <summary>
    /// Error handling controller
    /// </summary>
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Constructor for error controller
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="env">Web host environment</param>
        public ErrorController(ILogger<ErrorController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// Global error handler endpoint
        /// </summary>
        /// <returns>Error details</returns>
        [Route("/error")]
        public IActionResult Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error;

            if (exception != null)
            {
                _logger.LogError(exception, "An unhandled exception occurred.");
            }

            var errorResponse = new
            {
                Message = "An error occurred while processing your request.",
                TraceId = HttpContext.TraceIdentifier,
                
                // Only include exception details in development environment
                Error = _env.IsDevelopment() ? new
                {
                    ExceptionMessage = exception?.Message,
                    ExceptionType = exception?.GetType().Name,
                    StackTrace = exception?.StackTrace?.Split('\n')
                } : null
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
} 