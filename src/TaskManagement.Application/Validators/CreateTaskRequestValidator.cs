using FluentValidation;
using TaskManagement.Application.DTOs.Requests;

namespace TaskManagement.Application.Validators
{
    /// <summary>
    /// Validator for CreateTaskRequest
    /// </summary>
    public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
    {
        /// <summary>
        /// Initializes validation rules for CreateTaskRequest
        /// </summary>
        public CreateTaskRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Task name is required")
                .Length(3, 100).WithMessage("Task name must be between 3 and 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Task description is required")
                .MaximumLength(500).WithMessage("Task description cannot exceed 500 characters");

            RuleFor(x => x.AssignedTo)
                .MaximumLength(100).WithMessage("Assignee name cannot exceed 100 characters");
        }
    }
} 