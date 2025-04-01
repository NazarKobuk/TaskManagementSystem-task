using FluentValidation;
using TaskManagement.Application.DTOs.Requests;

namespace TaskManagement.Application.Validators
{
    /// <summary>
    /// Validator for AssignTaskRequest
    /// </summary>
    public class AssignTaskRequestValidator : AbstractValidator<AssignTaskRequest>
    {
        /// <summary>
        /// Initializes validation rules for AssignTaskRequest
        /// </summary>
        public AssignTaskRequestValidator()
        {
            RuleFor(x => x.Assignee)
                .NotEmpty().WithMessage("Assignee name is required")
                .Length(1, 100).WithMessage("Assignee name must be between 1 and 100 characters");
        }
    }
} 