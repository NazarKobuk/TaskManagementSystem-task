using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs.Requests
{
    /// <summary>
    /// Request DTO for creating a new task
    /// </summary>
    public class CreateTaskRequest
    {
        /// <summary>
        /// Name of the task
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of the task
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Optional user assignment for the task
        /// </summary>
        [StringLength(100)]
        public string AssignedTo { get; set; }
    }
} 