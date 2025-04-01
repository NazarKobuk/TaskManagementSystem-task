using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.Interfaces;
using TaskManagement.Api.Controllers;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Api.Tests.Controllers
{
    [TestFixture]
    public class TaskControllerTests
    {
        private Mock<ITaskService> _mockTaskService = null!;
        private Mock<ILogger<TaskController>> _mockLogger = null!;
        private TaskController _controller = null!;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Setup()
        {
            _mockTaskService = new Mock<ITaskService>();
            _mockLogger = new Mock<ILogger<TaskController>>();
            _controller = new TaskController(_mockTaskService.Object, _mockLogger.Object);
            _cancellationToken = CancellationToken.None;
        }

        [Test]
        public async Task GetAllTasks_ReturnsOkResult_WithTasks()
        {
            // Arrange
            var tasks = new List<TaskResponse>
            {
                new TaskResponse { Id = 1, Name = "Test Task 1", Description = "Description 1", Status = "NotStarted" },
                new TaskResponse { Id = 2, Name = "Test Task 2", Description = "Description 2", Status = "InProgress" }
            };

            _mockTaskService.Setup(service => service.GetAllTasksAsync(_cancellationToken))
                .ReturnsAsync(tasks);

            // Act
            var result = await _controller.GetAllTasks();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            
            var returnedTasks = okResult.Value as IEnumerable<TaskResponse>;
            Assert.IsNotNull(returnedTasks);
            CollectionAssert.AreEquivalent(tasks, returnedTasks);
        }

        [Test]
        public async Task GetAllTasks_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var exception = new Exception("Test exception");
            _mockTaskService.Setup(service => service.GetAllTasksAsync(_cancellationToken))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.GetAllTasks();

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.AreEqual("An error occurred while processing your request", objectResult.Value);
        }

        [Test]
        public async Task GetTaskById_TaskExists_ReturnsOkResult()
        {
            // Arrange
            var taskId = 1;
            var taskResponse = new TaskResponse 
            { 
                Id = taskId, 
                Name = "Test Task", 
                Description = "Test Description", 
                Status = "NotStarted" 
            };

            _mockTaskService.Setup(service => service.GetTaskByIdAsync(taskId, _cancellationToken))
                .ReturnsAsync(taskResponse);

            // Act
            var result = await _controller.GetTaskById(taskId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            
            var returnedTask = okResult.Value as TaskResponse;
            Assert.IsNotNull(returnedTask);
            Assert.AreEqual(taskId, returnedTask.Id);
        }

        [Test]
        public async Task GetTaskById_TaskDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var taskId = 999;
            _mockTaskService.Setup(service => service.GetTaskByIdAsync(taskId, _cancellationToken))
                .ReturnsAsync((TaskResponse?)null);

            // Act
            var result = await _controller.GetTaskById(taskId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual($"Task with ID {taskId} not found", notFoundResult.Value);
        }

        [Test]
        public async Task CreateTask_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var createRequest = new CreateTaskRequest
            {
                Name = "New Task",
                Description = "New Task Description",
                AssignedTo = "user@example.com"
            };

            var createdTask = new TaskResponse
            {
                Id = 1,
                Name = createRequest.Name,
                Description = createRequest.Description,
                Status = "NotStarted",
                AssignedTo = createRequest.AssignedTo
            };

            _mockTaskService.Setup(service => service.CreateTaskAsync(createRequest, _cancellationToken))
                .ReturnsAsync(createdTask);

            // Act
            var result = await _controller.CreateTask(createRequest);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var createdResult = result.Result as CreatedAtActionResult;
            
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            
            var returnedTask = createdResult.Value as TaskResponse;
            Assert.IsNotNull(returnedTask);
            Assert.AreEqual(createdTask.Id, returnedTask.Id);
            Assert.AreEqual(createdTask.Name, returnedTask.Name);
        }

        [Test]
        public async Task UpdateTaskStatus_TaskExists_ReturnsOkResult()
        {
            // Arrange
            var taskId = 1;
            var updateRequest = new UpdateTaskStatusRequest 
            { 
                NewStatus = Domain.Enums.TaskStatus.InProgress 
            };

            var updatedTask = new TaskResponse
            {
                Id = taskId,
                Name = "Test Task",
                Description = "Test Description",
                Status = "InProgress",
                AssignedTo = "user@example.com"
            };

            _mockTaskService.Setup(service => service.UpdateTaskStatusAsync(taskId, updateRequest, _cancellationToken))
                .ReturnsAsync(updatedTask);

            // Act
            var result = await _controller.UpdateTaskStatus(taskId, updateRequest);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            
            var returnedTask = okResult.Value as TaskResponse;
            Assert.IsNotNull(returnedTask);
            Assert.AreEqual(updatedTask.Status, returnedTask.Status);
        }

        [Test]
        public async Task UpdateTaskStatus_TaskDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var taskId = 999;
            var updateRequest = new UpdateTaskStatusRequest 
            { 
                NewStatus = Domain.Enums.TaskStatus.InProgress 
            };

            _mockTaskService.Setup(service => service.UpdateTaskStatusAsync(taskId, updateRequest, _cancellationToken))
                .ReturnsAsync((TaskResponse?)null);

            // Act
            var result = await _controller.UpdateTaskStatus(taskId, updateRequest);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual($"Task with ID {taskId} not found", notFoundResult.Value);
        }

        [Test]
        public async Task AssignTask_TaskExists_ReturnsOkResult()
        {
            // Arrange
            var taskId = 1;
            var assignRequest = new AssignTaskRequest { Assignee = "new.user@example.com" };

            var updatedTask = new TaskResponse
            {
                Id = taskId,
                Name = "Test Task",
                Description = "Test Description",
                Status = "NotStarted",
                AssignedTo = assignRequest.Assignee
            };

            _mockTaskService.Setup(service => service.AssignTaskAsync(taskId, assignRequest, _cancellationToken))
                .ReturnsAsync(updatedTask);

            // Act
            var result = await _controller.AssignTask(taskId, assignRequest);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            
            var returnedTask = okResult.Value as TaskResponse;
            Assert.IsNotNull(returnedTask);
            Assert.AreEqual(updatedTask.AssignedTo, returnedTask.AssignedTo);
        }

        [Test]
        public async Task AssignTask_TaskDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var taskId = 999;
            var assignRequest = new AssignTaskRequest { Assignee = "new.user@example.com" };

            _mockTaskService.Setup(service => service.AssignTaskAsync(taskId, assignRequest, _cancellationToken))
                .ReturnsAsync((TaskResponse?)null);

            // Act
            var result = await _controller.AssignTask(taskId, assignRequest);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual($"Task with ID {taskId} not found", notFoundResult.Value);
        }
    }
} 