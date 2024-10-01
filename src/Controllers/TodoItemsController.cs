using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TodoApi.Data;
using TodoApi.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TodoApi.DTO;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [Authorize(Roles = "Reader")]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ITodoItemService _todoItemService;

        public TodoItemsController(TodoContext context, UserManager<User> userManager,ITodoItemService todoItemService)
        {
            _context = context;
            _userManager = userManager;
            _todoItemService = todoItemService;
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {

            var userRoles= User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            Console.WriteLine($"User Email: {userEmail}");
            Console.WriteLine($"User Roles: {string.Join(", ", userRoles)}");

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(); // In case there's an issue getting the user ID
            }
            
                var items = await _todoItemService.GetTodoItems(userEmail, userRoles);
                
                return Ok(items);
            


        }

        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
        {
            

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail)) {

                return Unauthorized();
            }
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            
                var item = await _todoItemService.GetTodoItemById(id, userEmail, userRoles);
                if (item == null)
                {
                    return NotFound();
                }
                return Ok(item);
            




        }

        // PUT: api/TodoItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, PostTodoItemDTO postTodoItemDTO)
        {

            
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {

                return Unauthorized();
            }
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            
                var item = await _todoItemService.UpdateTodoItem(id, postTodoItemDTO, userEmail, userRoles);
                return Ok(item);

           

        }

        // POST: api/TodoItems
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem(PostTodoItemDTO postTodoItemDto)
        {
            // Get the current user's ID from the claims (ensure that this claim is present in your token)
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(); // In case there's an issue getting the user ID
            }

            
                var item = await _todoItemService.CreateTodoItem(postTodoItemDto, userEmail);
                return Ok(item);
            
            
        }

        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();


            }
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            
                await _todoItemService.DeleteTodoItem(id, userEmail, userRoles);
                return Ok();
                


            
        }

            private bool TodoItemExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}
