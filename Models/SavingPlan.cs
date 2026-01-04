using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerApp.Models
{
    public class SavingPlan
    {
        public int Id { get; set; }

        [ValidateNever]
        public string UserId { get; set; } = default!;

        [ValidateNever]
        public ApplicationUser User { get; set; } = default!;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Expected monthly income")]
        public decimal ExpectedMonthlyIncome { get; set; }

        
        [Range(0, double.MaxValue)]
        [Display(Name = "Planned monthly savings")]
        public decimal? PlannedMonthlySavings { get; set; }
        
        [StringLength(300)]
        public string? Notes { get; set; }
        
        public bool IsActive { get; set; }
    }
}
