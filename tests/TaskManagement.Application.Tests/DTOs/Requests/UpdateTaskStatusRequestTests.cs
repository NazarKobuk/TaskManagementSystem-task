using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using TaskManagement.Application.DTOs.Requests;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Tests.DTOs.Requests
{
    [TestFixture]
    public class UpdateTaskStatusRequestTests
    {
        [Test]
        public void UpdateTaskStatusRequest_WithValidData_IsValid()
        {
            // Arrange
            var request = new UpdateTaskStatusRequest
            {
                NewStatus = TaskStatus.InProgress
            };
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.True);
            Assert.That(validationResults, Is.Empty);
        }
        
        [Test]
        public void UpdateTaskStatusRequest_WithAllPossibleStatuses_IsValid()
        {
            // Testing all possible enum values
            foreach (TaskStatus status in System.Enum.GetValues(typeof(TaskStatus)))
            {
                // Arrange
                var request = new UpdateTaskStatusRequest
                {
                    NewStatus = status
                };
                
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                
                // Act
                var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
                
                // Assert
                Assert.That(isValid, Is.True, $"Status {status} should be valid");
                Assert.That(validationResults, Is.Empty);
            }
        }
        
        [Test]
        public void UpdateTaskStatusRequest_DefaultConstructor_SetsDefaultStatus()
        {
            // Arrange & Act
            var request = new UpdateTaskStatusRequest();
            
            // Assert
            Assert.That(request.NewStatus, Is.EqualTo(default(TaskStatus)));
            Assert.That(request.NewStatus, Is.EqualTo(TaskStatus.NotStarted));
        }
        
        [Test]
        public void UpdateTaskStatusRequest_WithInvalidStatus_IsNotValid()
        {
            // Arrange
            int invalidStatusValue = 999; // A value outside the valid enum range
            var request = new UpdateTaskStatusRequest
            {
                NewStatus = (TaskStatus)invalidStatusValue
            };
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.False, $"Status {invalidStatusValue} should be invalid");
            Assert.That(validationResults, Is.Not.Empty);
            Assert.That(validationResults[0].ErrorMessage, Does.Contain("Invalid task status"));
        }
    }
} 