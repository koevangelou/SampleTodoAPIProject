using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using TodoApi.DTO;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Services;
using TodoApi.Services.Caching;
using Xunit;

namespace  TodoApi.Tests.Services
{
    public class TodoItemServiceTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<ITodoItemRepository> _mockTodoItemRepository;
        private readonly TodoItemService _service;
        private readonly Mock<IRedisCacheService> _mockRedisCacheService;

        public TodoItemServiceTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            _mockTodoItemRepository = new Mock<ITodoItemRepository>();
            _mockRedisCacheService = new Mock<IRedisCacheService>();
            _service = new TodoItemService(_mockUserManager.Object, _mockTodoItemRepository.Object,_mockRedisCacheService.Object);
        }

        [Fact]
        public async Task GetTodoItems_ReturnsAllItemsForAdmin()
        {
            // Arrange
            var adminEmail = "admin@example.com";
            var adminRoles = new List<string> { "Admin" };
            var allItems = new List<TodoItem>
            {
                new TodoItem { Id = 1, Name = "Item 1", IsComplete = false, UserId = "1" },
                new TodoItem { Id = 2, Name = "Item 2", IsComplete = true, UserId = "2" }
            };

            _mockTodoItemRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allItems);
            _mockRedisCacheService.Setup(r => r.GetData<IEnumerable<TodoItem>>("TodoItems_" + adminEmail)).Returns((IEnumerable<TodoItem>)null);

            // Act
            var result = await _service.GetTodoItems(adminEmail, adminRoles);

            // Assert
            Assert.Equal(allItems, result);
            _mockTodoItemRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTodoItems_ReturnsUserItemsForNonAdmin()
        {
            // Arrange
            var userEmail = "user@example.com";
            var userRoles = new List<string> { "User" };
            var userId = "user1";
            var user = new User { Id = userId, Email = userEmail };
            var userItems = new List<TodoItem>
            {
                new TodoItem { Id = 1, Name = "User Item 1", IsComplete = false, UserId = userId },
                new TodoItem { Id = 2, Name = "User Item 2", IsComplete = true, UserId = userId }
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(userEmail)).ReturnsAsync(user);
            _mockTodoItemRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(userItems);
            _mockRedisCacheService.Setup(r => r.GetData<IEnumerable<TodoItem>>("TodoItems_" + userEmail)).Returns((IEnumerable<TodoItem>)null);

            // Act
            var result = await _service.GetTodoItems(userEmail, userRoles);

            // Assert
            Assert.Equal(userItems, result);
            _mockUserManager.Verify(um => um.FindByEmailAsync(userEmail), Times.Once);
            _mockTodoItemRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetTodoItems_ThrowsExceptionWhenUserNotFound()
        {
            // Arrange
            var userEmail = "nonexistent@example.com";
            var userRoles = new List<string> { "User" };

            _mockUserManager.Setup(um => um.FindByEmailAsync(userEmail)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetTodoItems(userEmail, userRoles));
            _mockUserManager.Verify(um => um.FindByEmailAsync(userEmail), Times.Once);
        }
        [Fact]
        public async Task GetTodoItemById_ReturnsItemForAdmin()
        {
            // Arrange
            var adminEmail = "admin@example.com";
            var adminRoles = new List<string> { "Admin" };
            var itemId = 1;
            var item = new TodoItem { Id = itemId, Name = "Item 1", IsComplete = false, UserId = "1" };

            _mockTodoItemRepository.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);

            // Act
            var result = await _service.GetTodoItemById(itemId, adminEmail, adminRoles);
            Assert.NotNull(result);
            Assert.Equal(item, result);
            _mockTodoItemRepository.Verify(r => r.GetByIdAsync(itemId), Times.Once);

        }
        [Fact]
        public async Task GetTodoItemById_ReturnsItemForUser()
        {
            // Arrange
            var userEmail = "user@example.com";
            var userRoles = new List<string> { "Reader" };
            var userId = "user1";
            var item = new TodoItem
            {
                Id = 1,
                Name = "User Item 1",
                IsComplete = false,
                UserId = userId
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(userEmail)).ReturnsAsync(new User { Id = userId });
            _mockTodoItemRepository.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(item);

            // Act
            var result = await _service.GetTodoItemById(item.Id, userEmail, userRoles);
            Assert.NotNull(result);
            Assert.Equal(item, result);
            _mockUserManager.Verify(um => um.FindByEmailAsync(userEmail), Times.Once);
            _mockTodoItemRepository.Verify(r => r.GetByIdAsync(item.Id), Times.Once);

        }
        [Fact]
        public async Task GetTodoItemById_ThrowsExceptionWhenUserNotFound()
        {
            // Arrange
            var userEmail = "user@example.com";
            var userRoles = new List<string> {
                "Reader" };
            var itemId = 1;
            var item = new TodoItem {
                Id = itemId,
                Name = "User Item 1",
                IsComplete = false,
                UserId = "user1"
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(userEmail)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetTodoItemById(itemId, userEmail, userRoles));
            _mockUserManager.Verify(um => um.FindByEmailAsync(userEmail), Times.Once);
        }

        [Fact]
        public async Task GetTodoItemById_ThrowsExceptionWhenUserNotAuthorized()
        {
            // Arrange
            var userEmail = "";
            var userRoles = new List<string> { "Reader" };
            var userId = "user1";
            var item = new TodoItem
            {
                Id = 1,
                Name = "User Item 1",
                IsComplete = false,
                UserId = "user2"
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(userEmail)).ReturnsAsync(new User { Id = userId });
            _mockTodoItemRepository.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(item);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetTodoItemById(item.Id, userEmail, userRoles));
            _mockUserManager.Verify(um => um.FindByEmailAsync(userEmail), Times.Once);
            _mockTodoItemRepository.Verify(r => r.GetByIdAsync(item.Id), Times.Once);

        }
        [Fact]
        public async Task UpdateTodoItem_ReturnsUpdatedItemForAdmin()
        {
            // Arrange
            var adminEmail = "";
            var adminRoles = new List<string> { "Admin" };
            var itemId = 1;
            var item = new TodoItem
            {
                Id = itemId,
                Name = "Item 1",
                IsComplete = false,
                UserId = "1"
            };
            var itemDTO = new PostTodoItemDTO
            {
                Name = "Updated Item 1",
                IsComplete = true
            };

            _mockTodoItemRepository.Setup(r => r.GetByIdAsync(itemId)).ReturnsAsync(item);

            // Act
            var result = await _service.UpdateTodoItem(itemId, itemDTO, adminEmail, adminRoles);
            Assert.NotNull(result);
            Assert.Equal(itemDTO.Name, result.Name);
            Assert.Equal(itemDTO.IsComplete, result.IsComplete);
            _mockTodoItemRepository.Verify(r => r.GetByIdAsync(itemId), Times.Once);
            _mockTodoItemRepository.Verify(r => r.UpdateAsync(item), Times.Once);
            _mockTodoItemRepository.Verify(r => r.UpdateAsync(It.Is<TodoItem>(i => i.Name == itemDTO.Name && i.IsComplete == itemDTO.IsComplete)), Times.Once);

        }


        }
    }