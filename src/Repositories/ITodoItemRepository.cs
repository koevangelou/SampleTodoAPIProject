using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface ITodoItemRepository
    {
        Task<TodoItem> AddAsync(TodoItem todoItem);
        Task DeleteAsync(TodoItem todoItem);
        Task<IEnumerable<TodoItem>> GetAllAsync();
        Task<TodoItem> GetByIdAsync(long id);
        Task<IEnumerable<TodoItem>> GetByUserIdAsync(string userId);
        Task UpdateAsync(TodoItem todoItem);
    }
}