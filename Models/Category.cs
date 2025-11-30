using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        // Name of the category
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        // Which user this category belongs to
        public string? UserId { get; set; }
        
        public ApplicationUser? User { get; set; }

        // Navigation property for related expenses
        public ICollection<Expense>? Expenses { get; set; }
    }
}
