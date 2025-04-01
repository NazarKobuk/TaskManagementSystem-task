namespace TaskManagement.ServiceBus.Configuration
{
    /// <summary>
    /// Settings for RabbitMQ connection
    /// </summary>
    public class RabbitMQSettings
    {
        /// <summary>
        /// The hostname of the RabbitMQ server
        /// </summary>
        public string HostName { get; set; } = "localhost";
        
        /// <summary>
        /// The port of the RabbitMQ server
        /// </summary>
        public int Port { get; set; } = 5672;
        
        /// <summary>
        /// The username for RabbitMQ authentication
        /// </summary>
        public string UserName { get; set; } = "guest";
        
        /// <summary>
        /// The password for RabbitMQ authentication
        /// </summary>
        public string Password { get; set; } = "guest";
        
        /// <summary>
        /// The virtual host to use
        /// </summary>
        public string VirtualHost { get; set; } = "/";
        
        /// <summary>
        /// Maximum retry attempts for connections
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 5;
        
        /// <summary>
        /// Initial interval between retries (in milliseconds)
        /// </summary>
        public int RetryIntervalMs { get; set; } = 1000;
    }
} 