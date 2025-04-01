namespace TaskManagement.Api.Middleware
{
    /// <summary>
    /// Extension methods for adding custom middleware to the application pipeline
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds request logging middleware to the pipeline
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder with middleware added</returns>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
} 