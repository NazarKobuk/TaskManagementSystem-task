using NUnit.Framework;
using TaskManagement.Domain.Events;

namespace TaskManagement.Domain.Tests.Events
{
    [TestFixture]
    public class TaskAssignedEventTests
    {
        [Test]
        public void TaskAssignedEvent_WhenCreated_HasDefaultValues()
        {
            // Arrange & Act
            var taskAssignedEvent = new TaskAssignedEvent();

            // Assert
            Assert.That(taskAssignedEvent.Id, Is.EqualTo(0));
            Assert.That(taskAssignedEvent.TaskName, Is.EqualTo(string.Empty));
            Assert.That(taskAssignedEvent.AssigneeId, Is.EqualTo(0));
            Assert.That(taskAssignedEvent.AssigneeName, Is.EqualTo(string.Empty));
            Assert.That(taskAssignedEvent.AssignedAt, Is.EqualTo(default(DateTimeOffset)));
            Assert.That(taskAssignedEvent.AssignedBy, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TaskAssignedEvent_WhenPropertiesSet_ReflectsChanges()
        {
            // Arrange
            var assignedAt = DateTimeOffset.UtcNow;
            var taskAssignedEvent = new TaskAssignedEvent
            {
                Id = 123,
                TaskName = "Task to Assign",
                AssigneeId = 456,
                AssigneeName = "John Doe",
                AssignedAt = assignedAt,
                AssignedBy = "Admin User"
            };

            // Assert
            Assert.That(taskAssignedEvent.Id, Is.EqualTo(123));
            Assert.That(taskAssignedEvent.TaskName, Is.EqualTo("Task to Assign"));
            Assert.That(taskAssignedEvent.AssigneeId, Is.EqualTo(456));
            Assert.That(taskAssignedEvent.AssigneeName, Is.EqualTo("John Doe"));
            Assert.That(taskAssignedEvent.AssignedAt, Is.EqualTo(assignedAt));
            Assert.That(taskAssignedEvent.AssignedBy, Is.EqualTo("Admin User"));
        }
    }
} 