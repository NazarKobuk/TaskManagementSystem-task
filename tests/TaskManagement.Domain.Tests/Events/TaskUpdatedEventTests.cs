using NUnit.Framework;
using TaskManagement.Domain.Events;

namespace TaskManagement.Domain.Tests.Events
{
    [TestFixture]
    public class TaskUpdatedEventTests
    {
        [Test]
        public void TaskUpdatedEvent_WhenCreated_HasDefaultValues()
        {
            // Arrange & Act
            var taskUpdatedEvent = new TaskUpdatedEvent();

            // Assert
            Assert.That(taskUpdatedEvent.Id, Is.EqualTo(0));
            Assert.That(taskUpdatedEvent.TaskName, Is.EqualTo(string.Empty));
            Assert.That(taskUpdatedEvent.Status, Is.EqualTo(string.Empty));
            Assert.That(taskUpdatedEvent.UpdatedAt, Is.EqualTo(default(DateTimeOffset)));
            Assert.That(taskUpdatedEvent.UpdatedBy, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TaskUpdatedEvent_WhenPropertiesSet_ReflectsChanges()
        {
            // Arrange
            var updatedAt = DateTimeOffset.UtcNow;
            var taskUpdatedEvent = new TaskUpdatedEvent
            {
                Id = 123,
                TaskName = "Updated Task",
                Status = "Completed",
                UpdatedAt = updatedAt,
                UpdatedBy = "Admin User"
            };

            // Assert
            Assert.That(taskUpdatedEvent.Id, Is.EqualTo(123));
            Assert.That(taskUpdatedEvent.TaskName, Is.EqualTo("Updated Task"));
            Assert.That(taskUpdatedEvent.Status, Is.EqualTo("Completed"));
            Assert.That(taskUpdatedEvent.UpdatedAt, Is.EqualTo(updatedAt));
            Assert.That(taskUpdatedEvent.UpdatedBy, Is.EqualTo("Admin User"));
        }
    }
} 