using System;

namespace TaskManagement.Domain.Events
{
    /// <summary>
    /// Event raised when a task is created
    /// </summary>
    public class TaskCreatedEvent
    {
        /// <summary>
        /// Task ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Task name
        /// </summary>
        public string TaskName { get; set; } = string.Empty;
        
        /// <summary>
        /// Task description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// When the task was created
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }
        
        /// <summary>
        /// Priority level of the task
        /// </summary>
        public int Priority { get; set; }
    }
} 