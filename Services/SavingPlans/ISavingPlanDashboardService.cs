
using ExpenseTrackerApp.Models.ViewModels;

namespace ExpenseTrackerApp.Services.SavingPlans
{
    public interface ISavingPlanDashboardService
    {
        Task<SavingPlanDashboardVM?> BuildAsync(int planId, string userId);
    }
}
