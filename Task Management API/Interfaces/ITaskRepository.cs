using Task_Management_API.Models;

namespace Task_Management_API.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(int id);
        Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetAllAsync(string? status, int pageNumber, int pageSize);
        Task<TaskItem> CreateAsync(TaskItem task);
        Task<TaskItem> UpdateAsync(TaskItem task);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TaskItem>> GetOverduePendingTasksAsync();
        Task UpdateRangeAsync(IEnumerable<TaskItem> tasks);
    }
}
