namespace ExpenseTrackerApp.Models.ViewModels
{
    public class ExpenseFiltersVM
    {
        public int? Year { get; set; }
        public int? Month { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? CategoryId { get; set; }

        public string Sort { get; set; } = "date_desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 30;
    }
}
