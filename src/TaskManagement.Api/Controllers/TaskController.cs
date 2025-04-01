using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Api.Controllers
{
    /// <summary>
    /// API Controller for Task operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Create, read, update and manage tasks")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TaskController> _logger;

        /// <summary>
        /// Constructor for TaskController
        /// </summary>
        /// <param name="taskService">Task service</param>
        /// <param name="logger">Logger</param>
        public TaskController(ITaskService taskService, ILogger<TaskController> logger)
        {
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all tasks
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/task
        ///
        /// </remarks>
        /// <param name="cancellationToken">Token for canceling the request</param>
        /// <returns>List of tasks</returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Retrieves all tasks",
            Description = "Retrieves a collection of all tasks in the system",
            OperationId = "GetAllTasks",
            Tags = new[] { "Tasks" }
        )]
        [ProducesResponseType(typeof(IEnumerable<TaskResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAllTasks(CancellationToken cancellationToken = default)
        {
            try
            {
                var tasks = await _taskService.GetAllTasksAsync(cancellationToken);
                return Ok(tasks);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Request to retrieve all tasks was canceled");
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tasks");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Get task by ID
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/task/5
        ///
        /// </remarks>
        /// <param name="id">Task ID</param>
        /// <param name="cancellationToken">Token for canceling the request</param>
        /// <returns>Task details</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Retrieves a specific task by id",
            Description = "Retrieves the details of a specific task by its unique identifier",
            OperationId = "GetTaskById",
            Tags = new[] { "Tasks" }
        )]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskResponse>> GetTaskById(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id, cancellationToken);
                
                if (task == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }
                
                return Ok(task);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Request to retrieve task with ID {TaskId} was canceled", id);
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task with ID {TaskId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Create a new task
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/task
        ///     {
        ///        "name": "Task Name",
        ///        "description": "Task Description",
        ///        "assignedTo": "john.doe@example.com"
        ///     }
        ///
        /// </remarks>
        /// <param name="request">Task creation request</param>
        /// <param name="cancellationToken">Token for canceling the request</param>
        /// <returns>Created task</returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Creates a new task",
            Description = "Creates a new task with the provided details",
            OperationId = "CreateTask",
            Tags = new[] { "Tasks" }
        )]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] CreateTaskRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var createdTask = await _taskService.CreateTaskAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Request to create task was canceled");
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Update task status
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/task/5/status
        ///     {
        ///        "status": "InProgress",
        ///        "requestedBy": "john.doe@example.com"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Task ID</param>
        /// <param name="request">Status update request</param>
        /// <param name="cancellationToken">Token for canceling the request</param>
        /// <returns>Updated task</returns>
        [HttpPut("{id}/status")]
        [SwaggerOperation(
            Summary = "Updates a task's status",
            Description = "Updates the status of an existing task",
            OperationId = "UpdateTaskStatus",
            Tags = new[] { "Tasks" }
        )]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskResponse>> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var updatedTask = await _taskService.UpdateTaskStatusAsync(id, request, cancellationToken);
                
                if (updatedTask == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }
                
                return Ok(updatedTask);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid task status provided for task {TaskId}", id);
                return BadRequest(ex.Message);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Request to update status for task with ID {TaskId} was canceled", id);
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status for task with ID {TaskId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Assign task to a user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/task/5/assign
        ///     {
        ///        "assignee": "jane.doe@example.com",
        ///        "requestedBy": "john.doe@example.com"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Task ID</param>
        /// <param name="request">Assignment request</param>
        /// <param name="cancellationToken">Token for canceling the request</param>
        /// <returns>Updated task</returns>
        [HttpPut("{id}/assign")]
        [SwaggerOperation(
            Summary = "Assigns a task to a user",
            Description = "Assigns an existing task to a specific user",
            OperationId = "AssignTask",
            Tags = new[] { "Tasks" }
        )]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TaskResponse>> AssignTask(int id, [FromBody] AssignTaskRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var updatedTask = await _taskService.AssignTaskAsync(id, request, cancellationToken);
                
                if (updatedTask == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }
                
                return Ok(updatedTask);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Request to assign task with ID {TaskId} was canceled", id);
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning task with ID {TaskId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Delete a task
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/task/5
        ///
        /// </remarks>
        /// <param name="id">Task ID</param>
        /// <param name="cancellationToken">Token for canceling the request</param>
        /// <returns>Success indicator</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Deletes a task",
            Description = "Deletes a task by its unique identifier",
            OperationId = "DeleteTask",
            Tags = new[] { "Tasks" }
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTask(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _taskService.DeleteTaskAsync(id, cancellationToken);
                
                if (!result)
                {
                    return NotFound($"Task with ID {id} not found");
                }
                
                return NoContent();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Request to delete task with ID {TaskId} was canceled", id);
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task with ID {TaskId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }
    }
} 