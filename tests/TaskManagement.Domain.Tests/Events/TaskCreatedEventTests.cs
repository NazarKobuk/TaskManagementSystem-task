using NUnit.Framework;
using TaskManagement.Domain.Events;

namespace TaskManagement.Domain.Tests.Events
{
    [TestFixture]
    public class TaskCreatedEventTests
    {
        [Test]
        public void TaskCreatedEvent_WhenCreated_HasDefaultValues()
        {
            // Arrange & Act
            var taskCreatedEvent = new TaskCreatedEvent();

            // Assert
            Assert.That(taskCreatedEvent.Id, Is.EqualTo(0));
            Assert.That(taskCreatedEvent.TaskName, Is.EqualTo(string.Empty));
            Assert.That(taskCreatedEvent.Description, Is.EqualTo(string.Empty));
            Assert.That(taskCreatedEvent.CreatedAt, Is.EqualTo(default(DateTimeOffset)));
            Assert.That(taskCreatedEvent.Priority, Is.EqualTo(0));
        }

        [Test]
        public void TaskCreatedEvent_WhenPropertiesSet_ReflectsChanges()
        {
            // Arrange
            var createdAt = DateTimeOffset.UtcNow;
            var taskCreatedEvent = new TaskCreatedEvent
            {
                Id = 123,
                TaskName = "New Task",
                Description = "Task Description",
                CreatedAt = createdAt,
                Priority = 2
            };

            // Assert
            Assert.That(taskCreatedEvent.Id, Is.EqualTo(123));
            Assert.That(taskCreatedEvent.TaskName, Is.EqualTo("New Task"));
            Assert.That(taskCreatedEvent.Description, Is.EqualTo("Task Description"));
            Assert.That(taskCreatedEvent.CreatedAt, Is.EqualTo(createdAt));
            Assert.That(taskCreatedEvent.Priority, Is.EqualTo(2));
        }
    }
} 