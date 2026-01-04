using ExpenseTrackerApp.Models.ViewModels;
using System.Threading.Tasks;

namespace ExpenseTrackerApp.Services.Home
{
    public interface IHomeService
    {
        Task<HomeDashboardVM> BuildAsync(string userId);
    }
}
