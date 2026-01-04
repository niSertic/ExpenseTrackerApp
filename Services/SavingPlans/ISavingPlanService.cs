
using ExpenseTrackerApp.Models.ViewModels;

namespace ExpenseTrackerApp.Services.SavingPlans
{
    public interface ISavingPlanService
    {
        Task<SavingPlanDashboardVM?> BuildAsync(int planId, string userId);
    }
}
