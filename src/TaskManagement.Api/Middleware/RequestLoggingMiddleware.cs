using System.Diagnostics;

namespace TaskManagement.Api.Middleware
{
    /// <summary>
    /// Middleware for logging HTTP requests
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        /// <summary>
        /// Constructor for RequestLoggingMiddleware
        /// </summary>
        /// <param name="next">Next request delegate in pipeline</param>
        /// <param name="logger">Logger instance</param>
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Process HTTP request
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Task representing middleware operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Request started {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                
                await _next(context);
                
                stopwatch.Stop();
                _logger.LogInformation("Request finished {Method} {Path} - Status: {StatusCode} ({ElapsedMs}ms)", 
                    context.Request.Method, context.Request.Path, 
                    context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Request failed {Method} {Path} ({ElapsedMs}ms)", 
                    context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
} 