using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data.Context;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Tests.Repositories
{
    [TestFixture]
    public class TaskRepositoryTests
    {
        private TaskManagementDbContext _dbContext;
        private TaskRepository _taskRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
                .UseInMemoryDatabase(databaseName: $"TaskDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new TaskManagementDbContext(options);

            _taskRepository = new TaskRepository(_dbContext);
        }

        [TearDown]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetAllTasksAsync_ReturnsAllTasks()
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
                }
            };

            await _dbContext.Tasks.AddRangeAsync(tasks);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetAllTasksAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.Any(t => t.Name == "Task 1"), Is.True);
            Assert.That(result.Any(t => t.Name == "Task 2"), Is.True);
        }

        [Test]
        public async Task GetTaskByIdAsync_TaskExists_ReturnsTask()
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
            var result = await _taskRepository.GetTaskByIdAsync(task.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(task.Id));
            Assert.That(result.Name, Is.EqualTo("Test Task"));
            Assert.That(result.Description, Is.EqualTo("Test Description"));
            Assert.That(result.Status, Is.EqualTo(TaskStatus.NotStarted));
            Assert.That(result.AssignedTo, Is.EqualTo("User1"));
        }

        [Test]
        public async Task GetTaskByIdAsync_TaskDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _taskRepository.GetTaskByIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddTaskAsync_ValidTask_AddsTaskAndReturnsIt()
        {
            // Arrange
            var task = new TaskItem
            {
                Name = "New Task",
                Description = "New Description",
                Status = TaskStatus.NotStarted,
                AssignedTo = "User1"
            };

            // Act
            var result = await _taskRepository.AddTaskAsync(task);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.Name, Is.EqualTo("New Task"));
            
            var dbTask = await _dbContext.Tasks.FindAsync(result.Id);
            Assert.That(dbTask, Is.Not.Null);
            Assert.That(dbTask.Name, Is.EqualTo("New Task"));
        }

        [Test]
        public async Task UpdateTaskAsync_TaskExists_UpdatesTask()
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

            task.Name = "Updated Task";
            task.Status = TaskStatus.InProgress;

            // Act
            var result = await _taskRepository.UpdateTaskAsync(task);

            // Assert
            Assert.That(result, Is.True);
            
            var updatedTask = await _dbContext.Tasks.FindAsync(task.Id);
            Assert.That(updatedTask, Is.Not.Null);
            Assert.That(updatedTask.Name, Is.EqualTo("Updated Task"));
            Assert.That(updatedTask.Status, Is.EqualTo(TaskStatus.InProgress));
        }

        [Test]
        public async Task DeleteTaskAsync_TaskExists_DeletesTask()
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
            var result = await _taskRepository.DeleteTaskAsync(task.Id);

            // Assert
            Assert.That(result, Is.True);
            
            var deletedTask = await _dbContext.Tasks.FindAsync(task.Id);
            Assert.That(deletedTask, Is.Null);
        }

        [Test]
        public async Task DeleteTaskAsync_TaskDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _taskRepository.DeleteTaskAsync(999);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteAsync_TaskExists_DeletesTask()
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
            var result = await _taskRepository.DeleteAsync(task);

            // Assert
            Assert.That(result, Is.True);
            
            var deletedTask = await _dbContext.Tasks.FindAsync(task.Id);
            Assert.That(deletedTask, Is.Null);
        }
    }
} 