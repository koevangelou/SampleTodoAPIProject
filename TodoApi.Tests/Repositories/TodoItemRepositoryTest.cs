using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Repositories;
using Xunit;

namespace TodoApi.Tests.Repositories
{
    public class TodoItemRepositoryTests
    {
        private readonly DbContextOptions<TodoContext> _options;

        public TodoItemRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllItems()
        {
            // Arrange
            using (var context = new TodoContext(_options))
            {
                context.TodoItems.AddRange(
                    new TodoItem { Id = 1, Name = "Item 1", IsComplete = false, UserId = "1" },
                    new TodoItem { Id = 2, Name = "Item 2", IsComplete = true, UserId = "2" }
                );
                context.SaveChanges();
            }

            // Act
            using (var context = new TodoContext(_options))
            {
                var repository = new TodoItemRepository(context);
                var result = await repository.GetAllAsync();

                // Assert
                Assert.Equal(2, result.Count());
            }
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsUserItems()
        {
            // Arrange
            var userId = "user1";
            using (var context = new TodoContext(_options))
            {
                context.TodoItems.AddRange(
                    new TodoItem { Id = 1, Name = "User 1 Item", IsComplete = false, UserId = userId },
                    new TodoItem { Id = 2, Name = "User 2 Item", IsComplete = true, UserId = "user2" }
                );
                context.SaveChanges();
            }

            // Act
            using (var context = new TodoContext(_options))
            {
                var repository = new TodoItemRepository(context);
                var result = await repository.GetByUserIdAsync(userId);

                // Assert
                Assert.Single(result);
                Assert.Equal("User 1 Item", result.First().Name);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectItem()
        {
            // Arrange
            long itemId = 1;
            using (var context = new TodoContext(_options))
            {
                context.TodoItems.Add(new TodoItem { Id = itemId, Name = "Test Item", IsComplete = false, UserId = "1" });
                context.SaveChanges();
            }

            // Act
            using (var context = new TodoContext(_options))
            {
                var repository = new TodoItemRepository(context);
                var result = await repository.GetByIdAsync(itemId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Test Item", result.Name);
            }
        }

        [Fact]
        public async Task AddAsync_AddsNewItem()
        {
            // Arrange
            var newItem = new TodoItem { Name = "New Item", IsComplete = false, UserId = "1" };

            // Act
            using (var context = new TodoContext(_options))
            {
                var repository = new TodoItemRepository(context);
                await repository.AddAsync(newItem);
            }

            // Assert
            using (var context = new TodoContext(_options))
            {
                Assert.Equal(1, context.TodoItems.Count());
                Assert.Equal("New Item", context.TodoItems.First().Name);
            }
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingItem()
        {
            // Arrange
            long itemId = 1;
            using (var context = new TodoContext(_options))
            {
                context.TodoItems.Add(new TodoItem { Id = itemId, Name = "Original Item", IsComplete = false, UserId = "1" });
                context.SaveChanges();
            }

            // Act
            using (var context = new TodoContext(_options))
            {
                var repository = new TodoItemRepository(context);
                var item = await repository.GetByIdAsync(itemId);
                item.Name = "Updated Item";
                item.IsComplete = true;
                await repository.UpdateAsync(item);
            }

            // Assert
            using (var context = new TodoContext(_options))
            {
                var updatedItem = context.TodoItems.Find(itemId);
                Assert.Equal("Updated Item", updatedItem.Name);
                Assert.True(updatedItem.IsComplete);
            }
        }
    }
}