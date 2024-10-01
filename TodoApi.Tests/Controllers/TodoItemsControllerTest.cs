using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.DTO;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Services;
using Xunit;

namespace TodoApi.Tests.Controllers
{
    public class TodoItemsControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly TodoContext _context;
        private readonly TodoItemsController _controller;
        private readonly Mock<ITodoItemService> _mockTodoItemService;
        private readonly TodoItemRepository _mockTodoItemRepository;

        public TodoItemsControllerTests()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new TodoContext(options);

            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            _mockTodoItemRepository = new TodoItemRepository(_context);
            _mockTodoItemService = new Mock<ITodoItemService>();

            // Create controller
            _controller = new TodoItemsController(_context, _mockUserManager.Object, _mockTodoItemService.Object);
        }

        private void SetupControllerContext(string email, List<string> roles)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetTodoItems_ReturnsAllItemsForAdmin()
        {
            // Arrange
            var user = new User { Id = "1", Email = "admin@example.com" };
            var items = new List<TodoItem>
            {
                new TodoItem { Id = 1, Name = "Item 1",IsComplete=false, UserId = "1" },
                new TodoItem { Id = 2, Name = "Item 2",IsComplete=true, UserId = "2" }
            };
            _context.TodoItems.AddRange(items);
            await _context.SaveChangesAsync();

            SetupControllerContext("admin@example.com", ["Admin"]);

            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockTodoItemService.Setup(s => s.GetTodoItems("admin@example.com", new List<string> { "Admin" }))
    .ReturnsAsync(items);

            // Act
            var result = await _controller.GetTodoItems();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TodoItem>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedItems = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(okResult.Value);
            Assert.Equal(2, returnedItems.Count());
        }

        [Fact]
        public async Task GetTodoItems_ReturnsOnlyUserItemsForNonAdmin()
        {
            // Arrange
            var user = new User { Id = "1", Email = "user@example.com" };
            var items = new List<TodoItem>
            {
                new TodoItem { Id = 1, Name = "Item 1", UserId = "1" },
                new TodoItem { Id = 2, Name = "Item 2", UserId = "2" }
            };
            _context.TodoItems.AddRange(items);
            await _context.SaveChangesAsync();

            SetupControllerContext("user@example.com", ["Reader"]);

            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockTodoItemService.Setup(s => s.GetTodoItems("user@example.com", new List<string> { "Reader" }))
                .ReturnsAsync(items.Where(i => i.UserId == "1"));

            // Act
            var result = await _controller.GetTodoItems();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TodoItem>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedItems = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(okResult.Value);
            Assert.Single(returnedItems);
            Assert.Equal("Item 1", returnedItems.First().Name);
        }

        [Fact]
        public async Task GetTodoItem_ReturnsItemForAdmin()
        {
            // Arrange
            var user = new User { Id = "1", Email = "admin@example.com" };
            var item = new TodoItem { Id = 1, Name = "Item 1", UserId = "2" };
            _context.TodoItems.Add(item);
            await _context.SaveChangesAsync();

            SetupControllerContext("admin@example.com", ["Admin"]);

            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockTodoItemService.Setup(s => s.GetTodoItemById(1, "admin@example.com", new List<string> { "Admin" }))
                .ReturnsAsync(item);

            // Act
            var result = await _controller.GetTodoItem(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TodoItem>>(result);
            var OkResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedItem = Assert.IsType<TodoItem>(OkResult.Value);
            Assert.Equal("Item 1", returnedItem.Name);
        }

        [Fact]
        public async Task GetTodoItem_ThrowsExceptionForNonAdminNonOwner()
        {
            // Arrange
            var user = new User { Id = "1", Email = "user@example.com" };
            var item = new TodoItem { Id = 1, Name = "Item 1", UserId = "2" };
            _context.TodoItems.Add(item);
            await _context.SaveChangesAsync();

            SetupControllerContext("user@example.com", ["Reader"]);

            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockTodoItemService.Setup(s => s.GetTodoItemById(1, "user@example.com", new List<string> { "Reader" })).Throws(new ArgumentException("User not authorized to view this item."));

            // Act
            // var result = await _controller.GetTodoItem(1);


            // Assert n Act
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetTodoItem(1));

        }


            [Fact]
        public async Task PutTodoItem_UpdatesItemForAdmin()
        {
            // Arrange
            var user = new User { Id = "1", Email = "admin@example.com" };
            var item = new TodoItem { Id = 1, Name = "Item 1", UserId = "2", IsComplete = false };
            _context.TodoItems.Add(item);
            await _context.SaveChangesAsync();

            SetupControllerContext("admin@example.com", ["Admin"]);

            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockTodoItemService.Setup(s => s.UpdateTodoItem(1, It.IsAny<PostTodoItemDTO>(), "admin@example.com", new List<string> { "Admin" }))
                .ReturnsAsync(new TodoItem { Id = 1, Name = "Updated Item 1", UserId = "2", IsComplete = true });

            var updatedItem = new PostTodoItemDTO { Name = "Updated Item 1", IsComplete = true };

            // Act
            var result = await _controller.PutTodoItem(1, updatedItem);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedItem.Name, ((TodoItem)((OkObjectResult)result).Value).Name);


        }

        [Fact]
        public async Task PutTodoItem_ThrowsExceptionForNonAdminNonOwner()
        {
            // Arrange
            var user = new User { Id = "1", Email = "user@example.com" };
            var item = new TodoItem { Id = 1, Name = "Item 1", UserId = "2", IsComplete = false };
            _context.TodoItems.Add(item);
            await _context.SaveChangesAsync();

            SetupControllerContext("user@example.com", ["Reader"]);

            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockTodoItemService.Setup(s => s.UpdateTodoItem(1, It.IsAny<PostTodoItemDTO>(), "user@example.com", new List<string> { "Reader" }))
                .Throws(new ArgumentException("User not authorized to view this item."));

            var updatedItem = new PostTodoItemDTO { Name = "Updated Item 1", IsComplete = true };

            // Act
            // var result = await _controller.PutTodoItem(1, updatedItem);

            //Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.PutTodoItem(1, updatedItem));


        }

        [Fact]
        public async Task PostTodoItem_CreatesNewItemForUser()
        {
            // Arrange
            var user = new User { Id = "1", Email = "user@example.com" };
            SetupControllerContext("user@example.com", ["Reader"]);
            var newItem = new PostTodoItemDTO { Name = "New Item", IsComplete = false };
            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockTodoItemService.Setup(s => s.CreateTodoItem(It.IsAny<PostTodoItemDTO>(), "user@example.com"))
                .ReturnsAsync(new TodoItem { Id = 1, Name = "New Item", UserId = "1" });



            // Act
            var result = await _controller.PostTodoItem(newItem);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TodoItem>>(result);
            var createdAtActionResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var createdItem = Assert.IsType<TodoItem>(createdAtActionResult.Value);
            Assert.Equal("New Item", createdItem.Name);
            Assert.Equal("1", createdItem.UserId);


        }

        [Fact]
        public async Task DeleteTodoItem_DeletesItemForAdmin()
        {
            // Arrange
            var user = new User { Id = "1", Email = "admin@example.com" };
            var item = new TodoItem { Id = 1, Name = "Item 1", UserId = "2" };
            _context.TodoItems.Add(item);
            await _context.SaveChangesAsync();

            SetupControllerContext("admin@example.com", ["Admin"]);

            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);



            // Act
            var result = await _controller.DeleteTodoItem(1);

            // Assert
            Assert.IsType<OkResult>(result);

        }

        [Fact]
        public async Task DeleteTodoItem_ThrowsExceptionForNonExistentItem()
        {
            // Arrange
            SetupControllerContext("admin@example.com", ["Admin"]);
            _mockTodoItemService.Setup(s => s.DeleteTodoItem(999, "admin@example.com", new List<string> { "Admin" }))
                .Throws(new ArgumentException("Item Not Found"));

            // Act
            //var result = await _controller.DeleteTodoItem(999);

            //Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteTodoItem(999));


        }
    }
}
