using NUnit.Framework;
using TaskManagement.Domain.Entities;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Domain.Tests.Entities
{
    [TestFixture]
    public class TaskItemTests
    {
        [Test]
        public void TaskItem_WhenCreated_HasPropertiesThatCanBeSet()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = 1,
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.NotStarted,
                AssignedTo = "User1"
            };

            // Assert
            Assert.That(taskItem.Id, Is.EqualTo(1));
            Assert.That(taskItem.Name, Is.EqualTo("Test Task"));
            Assert.That(taskItem.Description, Is.EqualTo("Test Description"));
            Assert.That(taskItem.Status, Is.EqualTo(TaskStatus.NotStarted));
            Assert.That(taskItem.AssignedTo, Is.EqualTo("User1"));
        }

        [Test]
        public void TaskItem_WhenModified_ReflectsChanges()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = 1,
                Name = "Original Task",
                Description = "Original Description",
                Status = TaskStatus.NotStarted,
                AssignedTo = "User1"
            };

            // Act
            taskItem.Name = "Updated Task";
            taskItem.Description = "Updated Description";
            taskItem.Status = TaskStatus.InProgress;
            taskItem.AssignedTo = "User2";

            // Assert
            Assert.That(taskItem.Name, Is.EqualTo("Updated Task"));
            Assert.That(taskItem.Description, Is.EqualTo("Updated Description"));
            Assert.That(taskItem.Status, Is.EqualTo(TaskStatus.InProgress));
            Assert.That(taskItem.AssignedTo, Is.EqualTo("User2"));
        }

        [Test]
        public void TaskStatus_Enum_HasExpectedValues()
        {
            // Assert
            Assert.That((int)TaskStatus.NotStarted, Is.EqualTo(0));
            Assert.That((int)TaskStatus.InProgress, Is.EqualTo(1));
            Assert.That((int)TaskStatus.Completed, Is.EqualTo(2));
        }
    }
} 