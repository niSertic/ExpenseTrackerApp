namespace ExpenseTrackerApp.Models.ViewModels
{
    public class ExpenseFiltersVM
    {
        public int? Year { get; set; }
        public int? Month { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? CategoryId { get; set; }
    }
}
