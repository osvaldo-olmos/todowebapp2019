using System;
using System.Threading.Tasks;
using AspNetCoreTodo.Models;

namespace AspNetCoreTodo.Services
{
    public class FakeTodoItemService : ITodoItemService
    {
        public Task<TodoItem[]> GetIncompleteItemsAsync(ApplicationUser user)
        {
            var item1 = new TodoItem
            {
                Title = "Aprender netcore",
                DueAt = DateTimeOffset.Now.AddDays(90)
            };            
            
            var item2 = new TodoItem
            {
                Title = "Aprender kitesurf",
                DueAt = DateTimeOffset.Now.AddDays(90)
            };

            return Task.FromResult(new TodoItem[]{ item1, item2});
        }
        public Task<bool> AddItemAsync(TodoItem newItem, ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkDoneAsync(Guid id, ApplicationUser user)
        {
            throw new NotImplementedException();
        }
    }
}