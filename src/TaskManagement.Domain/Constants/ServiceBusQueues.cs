namespace TaskManagement.Domain.Constants
{
    /// <summary>
    /// Constants for service bus queue names
    /// </summary>
    public static class ServiceBusQueues
    {
        /// <summary>
        /// Queue for task creation events
        /// </summary>
        public const string TaskCreated = "task-created";

        /// <summary>
        /// Queue for task update events
        /// </summary>
        public const string TaskUpdated = "task-updated";

        /// <summary>
        /// Queue for task assignment events
        /// </summary>
        public const string TaskAssigned = "task-assigned";
    }
} 