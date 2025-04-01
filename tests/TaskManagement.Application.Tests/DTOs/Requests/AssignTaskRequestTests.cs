using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using TaskManagement.Application.DTOs.Requests;

namespace TaskManagement.Application.Tests.DTOs.Requests
{
    [TestFixture]
    public class AssignTaskRequestTests
    {
        [Test]
        public void AssignTaskRequest_WithValidData_IsValid()
        {
            // Arrange
            var request = new AssignTaskRequest
            {
                Assignee = "John Doe"
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
        public void AssignTaskRequest_WithMissingAssignee_IsInvalid()
        {
            // Arrange
            var request = new AssignTaskRequest();
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Count, Is.EqualTo(1));
            Assert.That(validationResults[0].MemberNames, Contains.Item("Assignee"));
        }
        
        [Test]
        public void AssignTaskRequest_WithEmptyAssignee_IsInvalid()
        {
            // Arrange
            var request = new AssignTaskRequest
            {
                Assignee = string.Empty
            };
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Count, Is.EqualTo(1));
            Assert.That(validationResults[0].MemberNames, Contains.Item("Assignee"));
        }
        
        [Test]
        public void AssignTaskRequest_WithAssigneeTooLong_IsInvalid()
        {
            // Arrange
            var request = new AssignTaskRequest
            {
                Assignee = new string('A', 101)
            };
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Count, Is.EqualTo(1));
            Assert.That(validationResults[0].MemberNames, Contains.Item("Assignee"));
        }
    }
} 