

namespace ExpenseTrackerApp.Models.ViewModels
{
    public class ActiveSavingPlanBadgeVM
    {
        public int PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime EffectiveEnd { get; set; }

        public decimal SavingsToDate { get; set; }

        public decimal? GoalToDate { get; set; }
        public decimal? GoalRemaining { get; set; }
        
    }
}
