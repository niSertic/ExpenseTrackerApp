using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTrackerApp.Models
{
    public class Expense
    {
        public int Id { get; set; }

        // Which user this expense belongs to
        [Required]
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        // Category
        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        // When the money was spent
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        // How much
        [Required]
        [Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Optional description
        [StringLength(500)]
        public string? Description { get; set; }
    }
}
