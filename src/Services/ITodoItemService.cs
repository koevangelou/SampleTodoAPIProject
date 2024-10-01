using TodoApi.DTO;
using TodoApi.Models;

namespace TodoApi.Services
{
    public interface ITodoItemService
    {
        Task<TodoItem> CreateTodoItem(PostTodoItemDTO postTodoItemDTO, string userEmail);
        Task DeleteTodoItem(long id, string userEmail, List<string> userRoles);
        Task<TodoItem> GetTodoItemById(long id, string userEmail, List<string> userRoles);
        Task<IEnumerable<TodoItem>> GetTodoItems(string userEmail, List<string> userRoles);
        Task<TodoItem> UpdateTodoItem(long id, PostTodoItemDTO todoItemDTO, string userEmail, List<string> userRoles);
    }
}