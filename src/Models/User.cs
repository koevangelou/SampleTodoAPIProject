  using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace TodoApi.Models
{
    public class User : IdentityUser
    {

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public List<TodoItem>? TodoItems { get; set; }

       
        
    }
}