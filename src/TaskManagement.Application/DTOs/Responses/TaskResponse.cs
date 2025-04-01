using System;
using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.DTOs.Responses
{
    /// <summary>
    /// Response DTO for task operations
    /// </summary>
    public class TaskResponse
    {
        /// <summary>
        /// Task identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Task name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Task description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Current task status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// User the task is assigned to
        /// </summary>
        public string AssignedTo { get; set; }
    }
} 