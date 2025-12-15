namespace ExpenseTrackerApp.Models.ViewModels
{
    public class CategorySummaryVM
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }
}
