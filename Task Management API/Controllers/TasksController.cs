using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Management_API.DTOs;
using Task_Management_API.Interfaces;

namespace Task_Management_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

   
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var result = await _taskService.GetAllAsync(status, pageNumber, pageSize);
            return Ok(ApiResponse<PaginatedResponse<TaskResponseDto>>.SuccessResponse(result, "Tasks retrieved successfully."));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task == null)
                return NotFound(ApiResponse<object>.FailResponse($"Task with Id {id} not found.", 404));

            return Ok(ApiResponse<TaskResponseDto>.SuccessResponse(task, "Task retrieved successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            var created = await _taskService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<TaskResponseDto>.SuccessResponse(created, "Task created successfully.", 201));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
        {
            var updated = await _taskService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound(ApiResponse<object>.FailResponse($"Task with Id {id} not found.", 404));

            return Ok(ApiResponse<TaskResponseDto>.SuccessResponse(updated, "Task updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _taskService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.FailResponse($"Task with Id {id} not found.", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Task deleted successfully."));
        }
    }
}
