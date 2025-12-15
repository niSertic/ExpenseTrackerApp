using System.Threading.Tasks;
using ExpenseTrackerApp.Models.ViewModels;

namespace ExpenseTrackerApp.Services.Expenses
{
    public interface IExpensesDashboardService
    {
        Task<ExpensesIndexVM> BuildIndexAsync(string userId, ExpenseFiltersVM filters);
    }
}
