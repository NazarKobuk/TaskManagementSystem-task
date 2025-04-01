using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Domain.Entities
{
    /// <summary>
    /// Represents a task in the system
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// Unique identifier for the task
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the task
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of the task
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Current status of the task
        /// </summary>
        public TaskStatus Status { get; set; }

        /// <summary>
        /// Optional user assignment for the task
        /// </summary>
        public string AssignedTo { get; set; }
    }
} 