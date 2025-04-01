using System.ComponentModel.DataAnnotations;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.DTOs.Requests
{
    /// <summary>
    /// Request DTO for updating a task's status
    /// </summary>
    public class UpdateTaskStatusRequest
    {
        /// <summary>
        /// The new status for the task
        /// </summary>
        [Required]
        public TaskStatus NewStatus { get; set; }
    }
} 