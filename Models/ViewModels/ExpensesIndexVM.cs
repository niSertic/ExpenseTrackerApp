using System.Collections.Generic;


namespace ExpenseTrackerApp.Models.ViewModels
{
    public class ExpensesIndexVM
    {
        public ExpenseFiltersVM Filters { get; set; } = new();

        public List<Expense> Expenses { get; set; } = new();

        public decimal TotalAmount { get; set; }
        public int ExpenseCount { get; set; }
        public decimal AverageAmount { get; set; }

        public List<int> Years { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public List<CategorySummaryVM> CategorySummary { get; set; } = new();

        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartValues { get; set; } = new();

        public ActiveSavingPlanBadgeVM? ActivePlan { get; set; }

        public List<string> TrendLabels { get; set; } = new();
        public List<decimal> TrendValues { get; set; } = new();
    }
}
