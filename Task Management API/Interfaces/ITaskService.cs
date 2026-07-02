using Task_Management_API.DTOs;

namespace Task_Management_API.Interfaces
{
    public interface ITaskService
    {
        Task<TaskResponseDto?> GetByIdAsync(int id);
        Task<PaginatedResponse<TaskResponseDto>> GetAllAsync(string? status, int pageNumber, int pageSize);
        Task<TaskResponseDto> CreateAsync(CreateTaskDto dto);
        Task<TaskResponseDto?> UpdateAsync(int id, UpdateTaskDto dto);
        Task<bool> DeleteAsync(int id);
        Task MarkOverdueTasksAsExpiredAsync();
    }
}
