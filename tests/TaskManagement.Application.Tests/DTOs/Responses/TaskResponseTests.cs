using NUnit.Framework;
using TaskManagement.Application.DTOs.Responses;

namespace TaskManagement.Application.Tests.DTOs.Responses
{
    [TestFixture]
    public class TaskResponseTests
    {
        [Test]
        public void TaskResponse_DefaultConstructor_CreatesInstance()
        {
            // Act
            var response = new TaskResponse();
            
            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Id, Is.EqualTo(0));
            Assert.That(response.Name, Is.Null);
            Assert.That(response.Description, Is.Null);
            Assert.That(response.Status, Is.Null);
            Assert.That(response.AssignedTo, Is.Null);
        }
        
        [Test]
        public void TaskResponse_WithProperties_SetsProperties()
        {
            // Arrange
            var response = new TaskResponse
            {
                Id = 123,
                Name = "Task Name",
                Description = "Task Description",
                Status = "InProgress",
                AssignedTo = "John Doe"
            };
            
            // Assert
            Assert.That(response.Id, Is.EqualTo(123));
            Assert.That(response.Name, Is.EqualTo("Task Name"));
            Assert.That(response.Description, Is.EqualTo("Task Description"));
            Assert.That(response.Status, Is.EqualTo("InProgress"));
            Assert.That(response.AssignedTo, Is.EqualTo("John Doe"));
        }
        
        [Test]
        public void TaskResponse_ModifyProperties_ChangesValues()
        {
            // Arrange
            var response = new TaskResponse
            {
                Id = 1,
                Name = "Original Name",
                Description = "Original Description",
                Status = "NotStarted",
                AssignedTo = "Original User"
            };
            
            // Act
            response.Id = 2;
            response.Name = "Updated Name";
            response.Description = "Updated Description";
            response.Status = "Completed";
            response.AssignedTo = "New User";
            
            // Assert
            Assert.That(response.Id, Is.EqualTo(2));
            Assert.That(response.Name, Is.EqualTo("Updated Name"));
            Assert.That(response.Description, Is.EqualTo("Updated Description"));
            Assert.That(response.Status, Is.EqualTo("Completed"));
            Assert.That(response.AssignedTo, Is.EqualTo("New User"));
        }
    }
} 