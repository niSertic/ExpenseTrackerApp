using ExpenseTrackerApp.Models.ViewModels;
using System.Threading.Tasks;

namespace ExpenseTrackerApp.Services.Home
{
    public interface IHomeDashboardService
    {
        Task<HomeDashboardVM> BuildAsync(string userId);
    }
}
