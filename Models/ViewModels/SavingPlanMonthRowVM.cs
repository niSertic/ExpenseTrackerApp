namespace ExpenseTrackerApp.Models.ViewModels
{
    public class SavingPlanMonthRowVM
    {
        public int Year { get; set; }
        public int Month { get; set; }         
        public decimal Income { get; set; }
        public decimal Spent { get; set; }
        public decimal Savings => Income - Spent;
    }
}
