namespace ExpenseTrackerApp.Models
{
    public class CategorySummaryViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }
}
