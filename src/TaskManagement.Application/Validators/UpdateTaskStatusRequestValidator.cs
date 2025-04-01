using FluentValidation;
using TaskManagement.Application.DTOs.Requests;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Validators
{
    /// <summary>
    /// Validator for UpdateTaskStatusRequest
    /// </summary>
    public class UpdateTaskStatusRequestValidator : AbstractValidator<UpdateTaskStatusRequest>
    {
        /// <summary>
        /// Initializes validation rules for UpdateTaskStatusRequest
        /// </summary>
        public UpdateTaskStatusRequestValidator()
        {
            RuleFor(x => x.NewStatus)
                .Must(status => 
                {
                    var validValues = Enum.GetValues(typeof(TaskStatus)).Cast<TaskStatus>();
                    return validValues.Contains(status);
                })
                .WithMessage("Invalid task status value. Valid values are: NotStarted (0), InProgress (1), Completed (2).");
        }
    }
} 