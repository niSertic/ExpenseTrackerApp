using System;
using System.Collections.Generic;
using ExpenseTrackerApp.Models;

namespace ExpenseTrackerApp.Models.ViewModels
{
    public class SavingPlanDashboardVM
    {
        public SavingPlan Plan { get; set; } = default!;

        public DateTime EffectiveEnd { get; set; }
        public int MonthsElapsed { get; set; }

        public decimal IncomeToDate { get; set; }
        public decimal SpentToDate { get; set; }
        public decimal SavingsToDate { get; set; }

        public decimal? GoalToDate { get; set; }
        public decimal? ProgressPercent { get; set; }

        public List<SavingPlanMonthRowVM> MonthlyRows { get; set; } = new();
    }
}
