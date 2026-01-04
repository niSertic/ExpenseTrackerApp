using ExpenseTrackerApp.Data;
using ExpenseTrackerApp.Models.ViewModels;
using ExpenseTrackerApp.Services.SavingPlans;
using Microsoft.EntityFrameworkCore;


namespace ExpenseTrackerApp.Services.Home
{
    public class HomeService : IHomeService
    {

        private readonly ApplicationDbContext _context;
        private readonly ISavingPlanService _savingPlanDashboardService;

        public HomeService(
            ApplicationDbContext context,
            ISavingPlanService savingPlanDashboardService)
        {
            _context = context;
            _savingPlanDashboardService = savingPlanDashboardService;
        }

        public async Task<HomeDashboardVM> BuildAsync(string userId)
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // Current month stats
            var currentMonthQuery = _context.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId && e.Date >= monthStart);

            var currentMonthSpent =
                await currentMonthQuery.SumAsync(e => (decimal?)e.Amount) ?? 0m;

            var currentMonthCount =
                await currentMonthQuery.CountAsync();

            // Monthly trend 
            var trendLabels = new List<string>();
            var trendValues = new List<decimal>();

            var trendStart = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
            var grouped = await _context.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId && e.Date >= trendStart)
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            var cursor = trendStart;
            for (int i = 0; i < 6; i++)
            {
                var match = grouped.FirstOrDefault(x =>
                    x.Year == cursor.Year && x.Month == cursor.Month);

                trendLabels.Add($"{cursor.Year:D4}-{cursor.Month:D2}");
                trendValues.Add(match?.Total ?? 0m);

                cursor = cursor.AddMonths(1);
            }

            

            return new HomeDashboardVM
            {
                CurrentMonthSpent = currentMonthSpent,
                CurrentMonthExpenseCount = currentMonthCount,
                TrendLabels = trendLabels,
                TrendValues = trendValues
            };
        }
    }
}
