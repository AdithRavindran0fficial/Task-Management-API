using Task_Management_API.DTOs;
using Task_Management_API.Interfaces;
using Task_Management_API.Models;

namespace Task_Management_API.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly ILogger<TaskService> _logger;

        public TaskService(ITaskRepository repository, ILogger<TaskService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<TaskResponseDto?> GetByIdAsync(int id)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task == null)
                return null;

            return MapToDto(task);
        }

        public async Task<PaginatedResponse<TaskResponseDto>> GetAllAsync(string? status, int pageNumber, int pageSize)
        {
            if (!string.IsNullOrWhiteSpace(status) && !Enum.TryParse<TaskItemStatus>(status, true, out _))
            {
                throw new ArgumentException($"Invalid status value: '{status}'. Valid values are: Pending, Completed, Expired.");
            }

            var normalizedStatus = !string.IsNullOrWhiteSpace(status)
                ? Enum.Parse<TaskItemStatus>(status, true).ToString()
                : null;

            var (items, totalCount) = await _repository.GetAllAsync(normalizedStatus, pageNumber, pageSize);

            return new PaginatedResponse<TaskResponseDto>
            {
                Data = items.Select(MapToDto),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto)
        {
            if (dto.DueDate <= DateTime.UtcNow)
            {
                throw new ArgumentException("DueDate cannot be in the past.");
            }

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Status = TaskItemStatus.Pending.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(task);
            _logger.LogInformation("Task created with Id: {TaskId}", created.Id);

            return MapToDto(created);
        }

        public async Task<TaskResponseDto?> UpdateAsync(int id, UpdateTaskDto dto)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task == null)
                return null;

            if (!string.IsNullOrWhiteSpace(dto.Title))
                task.Title = dto.Title;

            if (dto.Description != null)
                task.Description = dto.Description;

            if (dto.DueDate.HasValue)
            {
                if (dto.DueDate.Value <= DateTime.UtcNow)
                    throw new ArgumentException("DueDate cannot be in the past.");
                task.DueDate = dto.DueDate.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                if (!Enum.TryParse<TaskItemStatus>(dto.Status, true, out var parsedStatus))
                    throw new ArgumentException($"Invalid status value: '{dto.Status}'. Valid values are: Pending, Completed, Expired.");
                task.Status = parsedStatus.ToString();
            }

            task.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(task);
            _logger.LogInformation("Task updated with Id: {TaskId}", updated.Id);

            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _repository.DeleteAsync(id);
            if (result)
                _logger.LogInformation("Task deleted with Id: {TaskId}", id);
            return result;
        }

        public async Task MarkOverdueTasksAsExpiredAsync()
        {
            var overdueTasks = await _repository.GetOverduePendingTasksAsync();
            var taskList = overdueTasks.ToList();

            if (taskList.Count == 0)
                return;

            foreach (var task in taskList)
            {
                task.Status = TaskItemStatus.Expired.ToString();
                task.UpdatedAt = DateTime.UtcNow;
            }

            await _repository.UpdateRangeAsync(taskList);
            _logger.LogInformation("Marked {Count} overdue tasks as Expired.", taskList.Count);
        }

        private static TaskResponseDto MapToDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}
