using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        public string? UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public ICollection<Expense>? Expenses { get; set; }
    }
}
