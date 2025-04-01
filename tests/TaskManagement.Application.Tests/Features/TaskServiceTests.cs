using Mapster;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.Features;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Events;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Tests.Helpers;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Tests.Features
{
    [TestFixture]
    public class TaskServiceTests
    {
        private Mock<ITaskRepository> _mockTaskRepository = null!;
        private Mock<IServiceBusHandler> _mockServiceBusHandler = null!;
        private Mock<ILogger<TaskService>> _mockLogger = null!;
        private TaskService _taskService = null!;

        [SetUp]
        public void Setup()
        {
            _mockTaskRepository = new Mock<ITaskRepository>();
            _mockServiceBusHandler = new Mock<IServiceBusHandler>();
            _mockLogger = new Mock<ILogger<TaskService>>();

            TypeAdapterConfig.GlobalSettings.Apply(new Application.Mappings.MappingRegister());

            _taskService = new TaskService(
                _mockTaskRepository.Object,
                _mockServiceBusHandler.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task GetAllTasksAsync_WhenTasksExist_ReturnsAllTasks()
        {
            // Arrange
            var taskItems = new List<TaskItem>
            {
                new TaskItem { Id = 1, Name = "Task 1", Description = "Description 1", Status = TaskStatus.NotStarted },
                new TaskItem { Id = 2, Name = "Task 2", Description = "Description 2", Status = TaskStatus.InProgress }
            };

            _mockTaskRepository.Setup(repo => repo.GetAllTasksAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskItems);

            // Act
            var result = await _taskService.GetAllTasksAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Name, Is.EqualTo("Task 1"));
            Assert.That(result.Last().Name, Is.EqualTo("Task 2"));

            _mockLogger.VerifyLog(LogLevel.Information, "Getting all tasks", Times.Once());
        }

        [Test]
        public async Task GetTaskByIdAsync_WhenTaskExists_ReturnsTask()
        {
            // Arrange
            var taskId = 1;
            var taskItem = new TaskItem
            {
                Id = taskId,
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.NotStarted
            };

            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskItem);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(taskId));
            Assert.That(result.Name, Is.EqualTo("Test Task"));
            Assert.That(result.Description, Is.EqualTo("Test Description"));
            Assert.That(result.Status, Is.EqualTo(TaskStatus.NotStarted.ToString()));

            _mockLogger.VerifyLog(LogLevel.Information, $"Getting task with id: {taskId}", Times.Once());
        }

        [Test]
        public async Task GetTaskByIdAsync_WhenTaskDoesNotExist_ReturnsNull()
        {
            // Arrange
            var taskId = 999;
            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskItem)null!);

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.That(result, Is.Null);

            _mockLogger.VerifyLog(LogLevel.Warning, $"Task with id {taskId} not found", Times.Once());
        }

        [Test]
        public async Task CreateTaskAsync_ValidRequest_CreatesTaskAndPublishesEvent()
        {
            // Arrange
            var request = new CreateTaskRequest
            {
                Name = "New Task",
                Description = "New Description",
                AssignedTo = "User1"
            };

            var createdTaskItem = new TaskItem
            {
                Id = 1,
                Name = request.Name,
                Description = request.Description,
                Status = TaskStatus.NotStarted,
                AssignedTo = request.AssignedTo
            };

            _mockTaskRepository.Setup(repo => repo.AddTaskAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTaskItem);

            // Act
            var result = await _taskService.CreateTaskAsync(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo(request.Name));
            Assert.That(result.Description, Is.EqualTo(request.Description));
            Assert.That(result.Status, Is.EqualTo(TaskStatus.NotStarted.ToString()));
            Assert.That(result.AssignedTo, Is.EqualTo(request.AssignedTo));

            _mockTaskRepository.Verify(repo => repo.AddTaskAsync(
                It.Is<TaskItem>(t => 
                    t.Name == request.Name && 
                    t.Description == request.Description && 
                    t.Status == TaskStatus.NotStarted), 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            _mockServiceBusHandler.Verify(sb => sb.SendMessageAsync(
                It.IsAny<TaskCreatedEvent>(),
                "task-created",
                It.IsAny<CancellationToken>()), 
                Times.Once);

            _mockLogger.VerifyLog(LogLevel.Information, $"Creating new task with name: {request.Name}", Times.Once());
        }

        [Test]
        public async Task UpdateTaskStatusAsync_WhenTaskExists_UpdatesStatusAndPublishesEvent()
        {
            // Arrange
            var taskId = 1;
            var request = new UpdateTaskStatusRequest { NewStatus = TaskStatus.Completed };
            
            var taskItem = new TaskItem
            {
                Id = taskId,
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.InProgress,
                AssignedTo = "User1"
            };

            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskItem);
            
            _mockTaskRepository.Setup(repo => repo.UpdateTaskAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Callback<TaskItem, CancellationToken>((t, _) => 
                {
                    t.Status = request.NewStatus;
                });

            // Act
            var result = await _taskService.UpdateTaskStatusAsync(taskId, request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(taskId));
            Assert.That(result.Status, Is.EqualTo(TaskStatus.Completed.ToString()));

            _mockTaskRepository.Verify(repo => repo.UpdateTaskAsync(
                It.Is<TaskItem>(t => t.Id == taskId && t.Status == TaskStatus.Completed),
                It.IsAny<CancellationToken>()), 
                Times.Once);

            _mockServiceBusHandler.Verify(sb => sb.SendMessageAsync(
                It.IsAny<TaskUpdatedEvent>(),
                "task-updated",
                It.IsAny<CancellationToken>()), 
                Times.Once);

            _mockLogger.VerifyLog(LogLevel.Information, $"Updating task {taskId} status to {request.NewStatus}", Times.Once());
        }

        [Test]
        public async Task UpdateTaskStatusAsync_WhenTaskDoesNotExist_ReturnsNull()
        {
            // Arrange
            var taskId = 999;
            var request = new UpdateTaskStatusRequest { NewStatus = TaskStatus.Completed };
            
            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskItem)null!);

            // Act
            var result = await _taskService.UpdateTaskStatusAsync(taskId, request);

            // Assert
            Assert.That(result, Is.Null);
            
            _mockTaskRepository.Verify(repo => repo.UpdateTaskAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockServiceBusHandler.Verify(sb => sb.SendMessageAsync(It.IsAny<TaskUpdatedEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            
            _mockLogger.VerifyLog(LogLevel.Warning, $"Task with id {taskId} not found", Times.Once());
        }

        [Test]
        public void UpdateTaskStatusAsync_WithInvalidStatus_ThrowsArgumentException()
        {
            // Arrange
            int taskId = 1;
            int invalidStatusValue = 999; // A value outside the valid enum range
            var request = new UpdateTaskStatusRequest 
            { 
                NewStatus = (TaskStatus)invalidStatusValue 
            };
            
            var taskItem = new TaskItem
            {
                Id = taskId,
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.InProgress
            };

            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskItem);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => 
                await _taskService.UpdateTaskStatusAsync(taskId, request));
            
            Assert.That(exception.Message, Does.Contain("Invalid task status"));
            _mockLogger.VerifyLog(LogLevel.Warning, $"Invalid status {invalidStatusValue} provided for task {taskId}", Times.Once());
            
            _mockTaskRepository.Verify(repo => repo.UpdateTaskAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockServiceBusHandler.Verify(sb => sb.SendMessageAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task AssignTaskAsync_WhenTaskExists_AssignsTaskAndPublishesEvent()
        {
            // Arrange
            var taskId = 1;
            var request = new AssignTaskRequest { Assignee = "John Doe" };
            
            var taskItem = new TaskItem
            {
                Id = taskId,
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.NotStarted,
                AssignedTo = "Original User"
            };

            var updatedTaskItem = new TaskItem
            {
                Id = taskId,
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.NotStarted,
                AssignedTo = request.Assignee
            };

            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskItem);
            
            _mockTaskRepository.Setup(repo => repo.UpdateTaskAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Callback<TaskItem, CancellationToken>((t, _) => 
                {
                    t.AssignedTo = request.Assignee;
                });

            // Act
            var result = await _taskService.AssignTaskAsync(taskId, request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(taskId));
            Assert.That(result.AssignedTo, Is.EqualTo(request.Assignee));

            _mockTaskRepository.Verify(repo => repo.UpdateTaskAsync(
                It.Is<TaskItem>(t => t.Id == taskId && t.AssignedTo == request.Assignee),
                It.IsAny<CancellationToken>()), 
                Times.Once);

            _mockServiceBusHandler.Verify(sb => sb.SendMessageAsync(
                It.IsAny<TaskAssignedEvent>(),
                "task-assigned",
                It.IsAny<CancellationToken>()), 
                Times.Once);

            _mockLogger.VerifyLog(LogLevel.Information, $"Assigning task {taskId} to {request.Assignee}", Times.Once());
        }

        [Test]
        public async Task AssignTaskAsync_WhenTaskDoesNotExist_ReturnsNull()
        {
            // Arrange
            var taskId = 999;
            var request = new AssignTaskRequest { Assignee = "John Doe" };
            
            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskItem)null!);

            // Act
            var result = await _taskService.AssignTaskAsync(taskId, request);

            // Assert
            Assert.That(result, Is.Null);
            
            _mockTaskRepository.Verify(repo => repo.UpdateTaskAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockServiceBusHandler.Verify(sb => sb.SendMessageAsync(It.IsAny<TaskAssignedEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            
            _mockLogger.VerifyLog(LogLevel.Warning, $"Task with id {taskId} not found", Times.Once());
        }

        [Test]
        public async Task DeleteTaskAsync_WhenTaskExists_DeletesTask()
        {
            // Arrange
            var taskId = 1;
            
            var taskItem = new TaskItem
            {
                Id = taskId,
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.NotStarted
            };
            
            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskItem);
                
            _mockTaskRepository.Setup(repo => repo.DeleteTaskAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId);

            // Assert
            Assert.That(result, Is.True);
            
            _mockTaskRepository.Verify(repo => repo.DeleteTaskAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
            _mockLogger.VerifyLog(LogLevel.Information, $"Deleting task with id: {taskId}", Times.Once());
        }

        [Test]
        public async Task DeleteTaskAsync_WhenTaskDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var taskId = 999;
            
            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskItem)null!);

            // Act
            var result = await _taskService.DeleteTaskAsync(taskId);

            // Assert
            Assert.That(result, Is.False);
            
            _mockTaskRepository.Verify(repo => repo.DeleteTaskAsync(taskId, It.IsAny<CancellationToken>()), Times.Never);
            _mockLogger.VerifyLog(LogLevel.Warning, $"Task with id {taskId} not found", Times.Once());
        }
    }
} 