using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoApi.DTO;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Services.Caching;

namespace TodoApi.Services
{
    public class TodoItemService : ITodoItemService
    {

        private readonly UserManager<User> _userManager;
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IRedisCacheService _redisCacheService;

        public TodoItemService(UserManager<User> userManager, ITodoItemRepository todoItemRepository,IRedisCacheService redisCacheService)
        {
            _userManager = userManager;
            _todoItemRepository = todoItemRepository;
            _redisCacheService = redisCacheService;
        }
        public async Task<IEnumerable<TodoItem>> GetTodoItems(string userEmail, List<string> userRoles)
        {
            if (userRoles.Contains("Admin"))
                
            {
                var cacheKey = $"TodoItems_{userEmail}";
                var cachedItems =  _redisCacheService.GetData<IEnumerable<TodoItem>>(cacheKey);
                if (cachedItems != null)
                {
                    return cachedItems;
                }
                var items= await _todoItemRepository.GetAllAsync();
                _redisCacheService.SetData(cacheKey, items, TimeSpan.FromMinutes(10));
                return items;
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    throw new ArgumentException("User not found.");
                }
                var cacheKey = $"TodoItems_{userEmail}";
                var cachedItems = _redisCacheService.GetData<IEnumerable<TodoItem>>(cacheKey);
                if (cachedItems != null)
                {
                    return cachedItems;
                }
                var items= await _todoItemRepository.GetByUserIdAsync(user.Id);
                _redisCacheService.SetData(cacheKey, items,TimeSpan.FromMinutes(10));
                return items;


            }
        }
        public async Task<TodoItem> GetTodoItemById(long id, string userEmail, List<string> userRoles)
        {
            if (userRoles.Contains("Admin"))
            {
                return await _todoItemRepository.GetByIdAsync(id);
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    throw new ArgumentException("User not found.");
                }

                var todoItem= await _todoItemRepository.GetByIdAsync(id);
                if (todoItem.UserId != user.Id)
                {
                    throw new ArgumentException("User not authorized to view this item.");
                }
                return todoItem;

            }

        }

        public async Task<TodoItem> UpdateTodoItem(long id, PostTodoItemDTO todoItemDTO, string userEmail, List<string> userRoles)
        {
            var todoItem = await _todoItemRepository.GetByIdAsync(id);
            if (todoItem == null)
            {
                throw new ArgumentException("Item Not Found");
            }

            if (userRoles.Contains("Admin"))
            {
                todoItem.Name = todoItemDTO.Name;
                todoItem.IsComplete = todoItemDTO.IsComplete;
                await _todoItemRepository.UpdateAsync(todoItem);

                return todoItem;
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    throw new ArgumentException("User not found.");
                }
                if (todoItem.UserId != user.Id)
                {
                    throw new ArgumentException("User not authorized to update this item.");
                }
                todoItem.Name = todoItemDTO.Name;
                todoItem.IsComplete = todoItemDTO.IsComplete;
                await _todoItemRepository.UpdateAsync(todoItem);
                return todoItem;

            }

        }

        public async Task<TodoItem> CreateTodoItem(PostTodoItemDTO postTodoItemDTO, string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }
            var todoItem = new TodoItem
            {
                Name = postTodoItemDTO.Name,
                IsComplete = postTodoItemDTO.IsComplete,
                UserId = user.Id
            };
            return await _todoItemRepository.AddAsync(todoItem);

        }
        public async Task DeleteTodoItem(long id, string userEmail, List<string> userRoles)
        {
            var todoItem = await _todoItemRepository.GetByIdAsync(id);
            if (todoItem == null)
            {
                throw new ArgumentException("Item Not Found");
            }

            if (userRoles.Contains("Admin"))
            {
                await _todoItemRepository.DeleteAsync(todoItem);
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    throw new ArgumentException("User not found.");
                }
                if (todoItem.UserId != user.Id)
                {
                    throw new ArgumentException("User not authorized to delete this item.");
                }
                await _todoItemRepository.DeleteAsync(todoItem);
            }
        }

    }
}
