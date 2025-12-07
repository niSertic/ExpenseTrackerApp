using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ExpenseTrackerApp.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [ValidateNever]
        public string UserId { get; set; } = default!;

        [ValidateNever]
        public ApplicationUser User { get; set; } = default!;

        // Category
        [Required]
        public int CategoryId { get; set; }

        [ValidateNever]
        public Category Category { get; set; } = default!;

        
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

       
        [Required]
        [Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        
        [StringLength(500)]
        public string? Description { get; set; }
    }
}
