using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data
{
    // Change the base class from DbContext to IdentityDbContext<User>
    public class TodoContext : IdentityDbContext<User>
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options)
        {
        }

        // DbSet for TodoItems - you already have Users through IdentityDbContext<User>
        public DbSet<TodoItem> TodoItems { get; set; }

       protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TodoItem>()
                .HasOne(t => t.User)
                .WithMany(u => u.TodoItems)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Set the delete behavior as needed



            base.OnModelCreating(modelBuilder);

            var readerRoleId = "a71a55d6-99d7-4123-b4e0-1218ecb90e3e";
            var writerRoleId = "c309fa92-2123-47be-b397-a1c77adb502c";
            var adminRoleId = "c309fa92-2323-47be-b397-a1c77adb502c";

            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = readerRoleId,
                    ConcurrencyStamp = readerRoleId,
                    Name = "Reader",
                    NormalizedName = "Reader".ToUpper()
                },
                new IdentityRole
                {
                    Id = writerRoleId,
                    ConcurrencyStamp = writerRoleId,
                    Name = "Writer",
                    NormalizedName = "Writer".ToUpper()
                },
                new IdentityRole
                {
                    Id = adminRoleId,
                    ConcurrencyStamp = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "Admin".ToUpper()
                }
            };

            modelBuilder.Entity<IdentityRole>().HasData(roles);
        }


    }
}
