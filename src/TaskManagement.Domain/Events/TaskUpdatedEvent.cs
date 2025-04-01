using System;

namespace TaskManagement.Domain.Events
{
    /// <summary>
    /// Event raised when a task is updated
    /// </summary>
    public class TaskUpdatedEvent
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
        /// New status of the task
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// When the task was updated
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }
        
        /// <summary>
        /// User who updated the task
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;
    }
} 