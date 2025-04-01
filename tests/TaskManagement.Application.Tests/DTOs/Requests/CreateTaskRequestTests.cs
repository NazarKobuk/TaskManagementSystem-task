using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using TaskManagement.Application.DTOs.Requests;

namespace TaskManagement.Application.Tests.DTOs.Requests
{
    [TestFixture]
    public class CreateTaskRequestTests
    {
        [Test]
        public void CreateTaskRequest_WithValidData_IsValid()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Name = "Test Task",
                Description = "This is a test task description",
                AssignedTo = "John Doe"
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
        public void CreateTaskRequest_WithNameTooShort_IsInvalid()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Name = "Ab",
                Description = "This is a test task description",
                AssignedTo = "John Doe"
            };
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Count, Is.EqualTo(1));
            Assert.That(validationResults[0].MemberNames, Contains.Item("Name"));
        }
        
        [Test]
        public void CreateTaskRequest_WithNameTooLong_IsInvalid()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Name = new string('A', 101),
                Description = "This is a test task description",
                AssignedTo = "John Doe"
            };
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Count, Is.EqualTo(1));
            Assert.That(validationResults[0].MemberNames, Contains.Item("Name"));
        }
        
        [Test]
        public void CreateTaskRequest_WithMissingName_IsInvalid()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Description = "This is a test task description",
                AssignedTo = "John Doe"
            };
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Count, Is.EqualTo(1));
            Assert.That(validationResults[0].MemberNames, Contains.Item("Name"));
        }
        
        [Test]
        public void CreateTaskRequest_WithMissingDescription_IsInvalid()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Name = "Test Task",
                AssignedTo = "John Doe"
            };
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Count, Is.EqualTo(1));
            Assert.That(validationResults[0].MemberNames, Contains.Item("Description"));
        }
        
        [Test]
        public void CreateTaskRequest_WithDescriptionTooLong_IsInvalid()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Name = "Test Task",
                Description = new string('A', 501),
                AssignedTo = "John Doe"
            };
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Count, Is.EqualTo(1));
            Assert.That(validationResults[0].MemberNames, Contains.Item("Description"));
        }
        
        [Test]
        public void CreateTaskRequest_WithAssignedToTooLong_IsInvalid()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Name = "Test Task",
                Description = "This is a test task description",
                AssignedTo = new string('A', 101) 
            };
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Count, Is.EqualTo(1));
            Assert.That(validationResults[0].MemberNames, Contains.Item("AssignedTo"));
        }
        
        [Test]
        public void CreateTaskRequest_WithNullAssignedTo_IsValid()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Name = "Test Task",
                Description = "This is a test task description",
                AssignedTo = null
            };
            
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
            
            // Assert
            Assert.That(isValid, Is.True);
            Assert.That(validationResults, Is.Empty);
        }
    }
} 