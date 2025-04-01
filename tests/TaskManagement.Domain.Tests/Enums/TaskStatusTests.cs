using NUnit.Framework;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Domain.Tests.Enums
{
    [TestFixture]
    public class TaskStatusTests
    {
        [Test]
        public void TaskStatus_Enum_HasExpectedValues()
        {
            // Assert
            Assert.That((int)TaskStatus.NotStarted, Is.EqualTo(0));
            Assert.That((int)TaskStatus.InProgress, Is.EqualTo(1));
            Assert.That((int)TaskStatus.Completed, Is.EqualTo(2));
        }

        [Test]
        public void TaskStatus_Enum_HasExpectedCount()
        {
            // Act
            var enumValues = System.Enum.GetValues(typeof(TaskStatus));
            
            // Assert
            Assert.That(enumValues.Length, Is.EqualTo(3), "TaskStatus enum should have exactly 3 values");
        }

        [Test]
        public void TaskStatus_Enum_CanBeConvertedToString()
        {
            // Assert
            Assert.That(TaskStatus.NotStarted.ToString(), Is.EqualTo("NotStarted"));
            Assert.That(TaskStatus.InProgress.ToString(), Is.EqualTo("InProgress"));
            Assert.That(TaskStatus.Completed.ToString(), Is.EqualTo("Completed"));
        }

        [Test]
        public void TaskStatus_Enum_CanBeParsedFromString()
        {
            // Act & Assert
            Assert.That(System.Enum.Parse<TaskStatus>("NotStarted"), Is.EqualTo(TaskStatus.NotStarted));
            Assert.That(System.Enum.Parse<TaskStatus>("InProgress"), Is.EqualTo(TaskStatus.InProgress));
            Assert.That(System.Enum.Parse<TaskStatus>("Completed"), Is.EqualTo(TaskStatus.Completed));
        }
    }
} 