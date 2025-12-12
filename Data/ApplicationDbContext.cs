using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExpenseTrackerApp.Models;

namespace ExpenseTrackerApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Food", UserId = null },
                new Category { Id = 2, Name = "Rent", UserId = null },
                new Category { Id = 3, Name = "Transport", UserId = null },
                new Category { Id = 4, Name = "Groceries", UserId = null },
                new Category { Id = 5, Name = "Entertainment", UserId = null },
                new Category { Id = 6, Name = "Utilities", UserId = null },
                new Category { Id = 7, Name = "Health", UserId = null },
                new Category { Id = 8, Name = "Shopping", UserId = null }
            );

            builder.Entity<Category>()
            .HasIndex(c => new { c.UserId, c.Name })
            .IsUnique();

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        }
}
