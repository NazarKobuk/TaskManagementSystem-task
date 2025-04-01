using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs.Requests
{
    /// <summary>
    /// Request DTO for assigning a task to a user
    /// </summary>
    public class AssignTaskRequest
    {
        /// <summary>
        /// The user to assign the task to
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Assignee { get; set; }
    }
} 