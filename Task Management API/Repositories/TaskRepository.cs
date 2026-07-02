using Microsoft.EntityFrameworkCore;
using Task_Management_API.Data;
using Task_Management_API.Interfaces;
using Task_Management_API.Models;

namespace Task_Management_API.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetAllAsync(string? status, int pageNumber, int pageSize)
        {
            var query = _context.Tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            query = query.OrderBy(t => t.DueDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem> UpdateAsync(TaskItem task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskItem>> GetOverduePendingTasksAsync()
        {
            var pendingStatus = TaskItemStatus.Pending.ToString();
            return await _context.Tasks
                .Where(t => t.Status == pendingStatus && t.DueDate < DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<TaskItem> tasks)
        {
            _context.Tasks.UpdateRange(tasks);
            await _context.SaveChangesAsync();
        }
    }
}
