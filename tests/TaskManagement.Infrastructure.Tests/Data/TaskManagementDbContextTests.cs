using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data.Context;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Tests.Data
{
    [TestFixture]
    public class TaskManagementDbContextTests
    {
        private TaskManagementDbContext _dbContext;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
                .UseInMemoryDatabase(databaseName: $"TaskDbContextTest_{Guid.NewGuid()}")
                .Options;

            _dbContext = new TaskManagementDbContext(options);
        }

        [TearDown]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public void TasksDbSet_ShouldBeAvailable()
        {
            // Arrange & Act
            var tasksDbSet = _dbContext.Tasks;

            // Assert
            Assert.That(tasksDbSet, Is.Not.Null);
        }

        [Test]
        public async Task SaveChangesAsync_SavesTaskEntityCorrectly()
        {
            // Arrange
            var task = new TaskItem
            {
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.NotStarted,
                AssignedTo = "User1"
            };

            // Act
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Assert
            var savedTask = await _dbContext.Tasks.FindAsync(task.Id);
            
            Assert.That(savedTask, Is.Not.Null);
            Assert.That(savedTask.Id, Is.GreaterThan(0));
            Assert.That(savedTask.Name, Is.EqualTo("Test Task"));
            Assert.That(savedTask.Description, Is.EqualTo("Test Description"));
            Assert.That(savedTask.Status, Is.EqualTo(TaskStatus.NotStarted));
            Assert.That(savedTask.AssignedTo, Is.EqualTo("User1"));
        }

        [Test]
        public async Task TasksDbSet_UpdatesTaskEntityCorrectly()
        {
            // Arrange
            var task = new TaskItem
            {
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.NotStarted,
                AssignedTo = "User1"
            };

            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Act
            task.Name = "Updated Task";
            task.Status = TaskStatus.InProgress;
            _dbContext.Tasks.Update(task);
            await _dbContext.SaveChangesAsync();

            // Assert
            var updatedTask = await _dbContext.Tasks.FindAsync(task.Id);
            
            Assert.That(updatedTask, Is.Not.Null);
            Assert.That(updatedTask.Name, Is.EqualTo("Updated Task"));
            Assert.That(updatedTask.Status, Is.EqualTo(TaskStatus.InProgress));
        }
        
        [Test]
        public async Task TasksDbSet_RemovesTaskEntityCorrectly()
        {
            // Arrange
            var task = new TaskItem
            {
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.NotStarted,
                AssignedTo = "User1"
            };

            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Act
            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();

            // Assert
            var deletedTask = await _dbContext.Tasks.FindAsync(task.Id);
            Assert.That(deletedTask, Is.Null);
        }

        [Test]
        public async Task TasksDbSet_QueryingWithLinqWorksCorrectly()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Name = "Task 1",
                    Description = "Description 1",
                    Status = TaskStatus.NotStarted,
                    AssignedTo = "User1"
                },
                new TaskItem
                {
                    Name = "Task 2",
                    Description = "Description 2",
                    Status = TaskStatus.InProgress,
                    AssignedTo = "User2"
                },
                new TaskItem
                {
                    Name = "Task 3",
                    Description = "Description 3",
                    Status = TaskStatus.Completed,
                    AssignedTo = "User1"
                }
            };

            await _dbContext.Tasks.AddRangeAsync(tasks);
            await _dbContext.SaveChangesAsync();

            // Act
            var completedTasks = await _dbContext.Tasks
                .Where(t => t.Status == TaskStatus.Completed)
                .ToListAsync();

            var userTasks = await _dbContext.Tasks
                .Where(t => t.AssignedTo == "User1")
                .ToListAsync();

            // Assert
            Assert.That(completedTasks, Is.Not.Null);
            Assert.That(completedTasks.Count, Is.EqualTo(1));
            Assert.That(completedTasks[0].Name, Is.EqualTo("Task 3"));

            Assert.That(userTasks, Is.Not.Null);
            Assert.That(userTasks.Count, Is.EqualTo(2));
            Assert.That(userTasks.Any(t => t.Name == "Task 1"), Is.True);
            Assert.That(userTasks.Any(t => t.Name == "Task 3"), Is.True);
        }
    }
} 