namespace ExpenseTrackerApp.Models.ViewModels
{
    public class HomeDashboardVM
    {
        public decimal CurrentMonthSpent { get; set; }
        public int CurrentMonthExpenseCount { get; set; }

        public List<string> TrendLabels { get; set; } = new();
        public List<decimal> TrendValues { get; set; } = new();
    }
}
