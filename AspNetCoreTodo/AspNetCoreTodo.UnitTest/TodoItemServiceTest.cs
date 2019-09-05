using System;
using System.Threading.Tasks;
using AspNetCoreTodo.Data;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetCoreTodo.UnitTest
{
    public class TodoItemServiceTest
    {
        [Fact]
        public async Task AddNewItemAsIncompleteWithDueDate()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Test_AddNewItem").Options;
            var todoItem = new TodoItem
            {
                Title = "Testing"
            };

            using(var context = new ApplicationDbContext(options)){
                var service = new TodoItemService(context);

                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@example.com"
                };

                await service.AddItemAsync(todoItem, fakeUser);
            }
            ClearDataBase(options);
        }

        [Fact]
        public async Task AddNewItemAsIncompleteWithDueDateSetByUser()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Test_AddNewItem").Options;
            var dueAt = DateTime.Today.AddDays(10);
            var todoItem = new TodoItem
            {
                Title = "Testing",
                DueAt = dueAt
            };

            using(var context = new ApplicationDbContext(options)){
                var service = new TodoItemService(context);

                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@example.com"
                };

                await service.AddItemAsync(todoItem, fakeUser);
            }

            using (var context = new ApplicationDbContext(options)){
                var itemsInDatabase = await context.Items.CountAsync();

                Assert.Equal(1, itemsInDatabase);

                var item = await context.Items.FirstAsync();

                Assert.Equal("Testing", item.Title);
                Assert.False(item.IsDone);
                Assert.Equal(dueAt, item.DueAt);
            }

            ClearDataBase(options);
        }

        [Fact]
        public async Task GetIncompleteItemsAsync_ShouldReturnOneTodoItem()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("Test_AddNewItem").Options;
            DateTime dueAt = DateTime.Today.AddDays(5);
            ApplicationUser userWithIncompletedItems = CreateFakeUser("fake-000", "fake@example.com");
            ApplicationUser otherUser = CreateFakeUser("fake-001", "fake1@example.com");

            TodoItem todoItemCompleted = CreateTodoItem("TodoItemCompleted", dueAt.AddDays(-10), userWithIncompletedItems.Id, true);
            TodoItem todoItemIncompleted = CreateTodoItem("TodoItemIncompleted", dueAt, userWithIncompletedItems.Id);
            TodoItem todoItemFromOtherUser = CreateTodoItem("TodoItemFromOtherUser", dueAt.AddDays(1), otherUser.Id);

            using (var context = new ApplicationDbContext(options))
            {
                await context.Items.AddAsync(todoItemCompleted);
                await context.Items.AddAsync(todoItemIncompleted);
                await context.Items.AddAsync(todoItemFromOtherUser);

                await context.SaveChangesAsync();
            }


            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                var todoItemsIncompletedForUser = await service.GetIncompleteItemsAsync(userWithIncompletedItems);

                Assert.Equal(1, todoItemsIncompletedForUser.Length);
                var todoItem = todoItemsIncompletedForUser[0];
                Assert.Equal(todoItemIncompleted.Title, todoItem.Title);
                Assert.False(todoItem.IsDone);
                Assert.Equal(todoItemIncompleted.DueAt, todoItem.DueAt);
                Assert.Equal(todoItemIncompleted.UserId, todoItem.UserId);
            }

            ClearDataBase(options);
        }

        private static ApplicationUser CreateFakeUser(string id, string email)
        {
            return new ApplicationUser
            {
                Id = id,
                UserName = email
            };
        }

        private static TodoItem CreateTodoItem(string title, DateTime dueAt, string userId, bool completed = false)
        {
            return new TodoItem
            {
                Title = title,
                DueAt = dueAt,
                UserId = userId,
                IsDone = completed
            };
        }

        private async void ClearDataBase(DbContextOptions<AspNetCoreTodo.Data.ApplicationDbContext> options){
            using (var context = new ApplicationDbContext(options)){
                await context.Database.EnsureDeletedAsync();
            }
        }
    }
}