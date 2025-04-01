using System;

namespace TaskManagement.Domain.Events
{
    /// <summary>
    /// Event raised when a task is assigned to a user
    /// </summary>
    public class TaskAssignedEvent
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
        /// User ID the task is assigned to
        /// </summary>
        public int AssigneeId { get; set; }
        
        /// <summary>
        /// Username the task is assigned to
        /// </summary>
        public string AssigneeName { get; set; } = string.Empty;
        
        /// <summary>
        /// When the task was assigned
        /// </summary>
        public DateTimeOffset AssignedAt { get; set; }
        
        /// <summary>
        /// User who assigned the task
        /// </summary>
        public string AssignedBy { get; set; } = string.Empty;
    }
} 