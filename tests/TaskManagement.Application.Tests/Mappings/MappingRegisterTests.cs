using Mapster;
using NUnit.Framework;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.Mappings;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Events;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Tests.Mappings
{
    [TestFixture]
    public class MappingRegisterTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            TypeAdapterConfig.GlobalSettings.Apply(new MappingRegister());
        }
        
        [Test]
        public void Map_TaskItemToTaskResponse_MapsCorrectly()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = 1,
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.InProgress,
                AssignedTo = "John Doe"
            };
            
            // Act
            var response = taskItem.Adapt<TaskResponse>();
            
            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Id, Is.EqualTo(taskItem.Id));
            Assert.That(response.Name, Is.EqualTo(taskItem.Name));
            Assert.That(response.Description, Is.EqualTo(taskItem.Description));
            Assert.That(response.Status, Is.EqualTo(taskItem.Status.ToString()));
            Assert.That(response.AssignedTo, Is.EqualTo(taskItem.AssignedTo));
        }
        
        [Test]
        public void Map_CreateTaskRequestToTaskItem_MapsCorrectly()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Name = "New Task",
                Description = "New Task Description",
                AssignedTo = "John Doe"
            };
            
            // Act
            var taskItem = request.Adapt<TaskItem>();
            
            // Assert
            Assert.That(taskItem, Is.Not.Null);
            Assert.That(taskItem.Name, Is.EqualTo(request.Name));
            Assert.That(taskItem.Description, Is.EqualTo(request.Description));
            Assert.That(taskItem.Status, Is.EqualTo(TaskStatus.NotStarted));
            Assert.That(taskItem.AssignedTo, Is.EqualTo(request.AssignedTo));
        }
        
        [Test]
        public void Map_TaskItemToTaskCreatedEvent_MapsCorrectly()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = 1,
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.NotStarted,
                AssignedTo = "Test User"
            };

            // Act
            var taskCreatedEvent = taskItem.Adapt<TaskCreatedEvent>();

            // Assert
            Assert.That(taskCreatedEvent.Id, Is.EqualTo(taskItem.Id));
            Assert.That(taskCreatedEvent.TaskName, Is.EqualTo(taskItem.Name));
            Assert.That(taskCreatedEvent.Description, Is.EqualTo(taskItem.Description));
            Assert.That(taskCreatedEvent.CreatedAt, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(taskCreatedEvent.CreatedAt, Is.GreaterThanOrEqualTo(DateTimeOffset.UtcNow.AddSeconds(-1)));
            Assert.That(taskCreatedEvent.CreatedAt, Is.LessThanOrEqualTo(DateTimeOffset.UtcNow));
        }
        
        [Test]
        public void Map_TaskItemToTaskUpdatedEvent_MapsCorrectly()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = 1,
                Name = "Test Task",
                Status = TaskStatus.Completed
            };

            // Act
            var taskUpdatedEvent = taskItem.Adapt<TaskUpdatedEvent>();

            // Assert
            Assert.That(taskUpdatedEvent.Id, Is.EqualTo(taskItem.Id));
            Assert.That(taskUpdatedEvent.TaskName, Is.EqualTo(taskItem.Name));
            Assert.That(taskUpdatedEvent.Status, Is.EqualTo(taskItem.Status.ToString()));
            Assert.That(taskUpdatedEvent.UpdatedAt, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(taskUpdatedEvent.UpdatedAt, Is.GreaterThanOrEqualTo(DateTimeOffset.UtcNow.AddSeconds(-1)));
            Assert.That(taskUpdatedEvent.UpdatedAt, Is.LessThanOrEqualTo(DateTimeOffset.UtcNow));
        }
        
        [Test]
        public void Map_TaskItemToTaskAssignedEvent_MapsCorrectly()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = 1,
                Name = "Test Task",
                AssignedTo = "New User"
            };

            // Act
            var taskAssignedEvent = taskItem.Adapt<TaskAssignedEvent>();

            // Assert
            Assert.That(taskAssignedEvent.Id, Is.EqualTo(taskItem.Id));
            Assert.That(taskAssignedEvent.TaskName, Is.EqualTo(taskItem.Name));
            Assert.That(taskAssignedEvent.AssigneeName, Is.EqualTo(taskItem.AssignedTo));
            Assert.That(taskAssignedEvent.AssignedAt, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(taskAssignedEvent.AssignedAt, Is.GreaterThanOrEqualTo(DateTimeOffset.UtcNow.AddSeconds(-1)));
            Assert.That(taskAssignedEvent.AssignedAt, Is.LessThanOrEqualTo(DateTimeOffset.UtcNow));
        }
    }
} 