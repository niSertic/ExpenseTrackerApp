using ExpenseTrackerApp.Data;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Models.ViewModels;
using ExpenseTrackerApp.Services.SavingPlans;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTrackerApp.Services.Expenses
{
    public class ExpensesService : IExpensesService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISavingPlanService _savingPlanDashboardService;

        public ExpensesService(ApplicationDbContext context, ISavingPlanService savingPlanDashboardService)
        {
            _context = context;
            _savingPlanDashboardService = savingPlanDashboardService;
        }

        public async Task<ExpensesIndexVM> BuildIndexAsync(string userId, ExpenseFiltersVM filters)
        {
            // Dropdowns
            var years = await _context.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .Select(e => e.Date.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Filtering
            var filtered = _context.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId);

            if (filters.Year.HasValue)
                filtered = filtered.Where(e => e.Date.Year == filters.Year.Value);

            if (filters.Month.HasValue)
                filtered = filtered.Where(e => e.Date.Month == filters.Month.Value);

            if (filters.From.HasValue)
                filtered = filtered.Where(e => e.Date.Date >= filters.From.Value.Date);

            if (filters.To.HasValue)
                filtered = filtered.Where(e => e.Date.Date <= filters.To.Value.Date);

            if (filters.CategoryId.HasValue)
                filtered = filtered.Where(e => e.CategoryId == filters.CategoryId.Value);


            // TotalItems for pagination 
            var totalItems = await filtered.CountAsync();

            // Sorting
            IQueryable<Expense> sorted = filtered;

            switch (filters.Sort?.ToLowerInvariant())
            {
                case "date_asc":
                    sorted = sorted.OrderBy(e => e.Date).ThenBy(e => e.Id);
                    break;
                case "amount_asc":
                    sorted = sorted.OrderBy(e => e.Amount).ThenByDescending(e => e.Date).ThenBy(e => e.Id);
                    break;
                case "amount_desc":
                    sorted = sorted.OrderByDescending(e => e.Amount).ThenByDescending(e => e.Date).ThenBy(e => e.Id);
                    break;
                case "date_desc":
                default:
                    sorted = sorted.OrderByDescending(e => e.Date).ThenByDescending(e => e.Id);
                    break;
            }

            // Paging (table only)
            var page = filters.Page < 1 ? 1 : filters.Page;
            var pageSize = filters.PageSize <= 0 ? 30 : filters.PageSize;
            var skip = (page - 1) * pageSize;

            // Paged expenses
            var expenses = await sorted
                .Include(e => e.Category)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Summary statistics
            var total = await filtered.SumAsync(e => (decimal?)e.Amount) ?? 0m;
            var count = totalItems;
            var average = count > 0 ? await filtered.AverageAsync(e => (decimal?)e.Amount) ?? 0m : 0m;

            // Category summary 
            var categorySummary = new List<CategorySummaryVM>();
            var chartLabels = new List<string>();
            var chartValues = new List<decimal>();

            if (!filters.CategoryId.HasValue)
            {
                categorySummary = await filtered
                    .Join(_context.Categories.AsNoTracking(),
                        e => e.CategoryId,
                        c => c.Id,
                        (e, c) => new { e.Amount, c.Name })
                    .GroupBy(x => x.Name)
                    .Select(g => new CategorySummaryVM
                    {
                        CategoryName = g.Key,
                        TotalAmount = g.Sum(x => x.Amount),
                        Percentage = total > 0 ? (g.Sum(x => x.Amount) / total) * 100m : 0m
                    })
                    .OrderByDescending(s => s.TotalAmount)
                    .ToListAsync();

                chartLabels = categorySummary.Select(s => s.CategoryName).ToList();
                chartValues = categorySummary.Select(s => s.TotalAmount).ToList();
            }

            // Monthly trend 
            var trendLabels = new List<string>();
            var trendValues = new List<decimal>();

           
            if (!filters.Month.HasValue)
            {
                DateTime start;
                DateTime end;

                if (filters.From.HasValue || filters.To.HasValue)
                {
                    start = filters.From ?? filters.To!.Value;
                    end = filters.To ?? filters.From!.Value;
                }
                else if (filters.Year.HasValue)
                {
                    start = new DateTime(filters.Year.Value, 1, 1);
                    end = new DateTime(filters.Year.Value, 12, 31);
                }
                else
                {
                    
                    start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11);
                    end = DateTime.Today;
                }

                
                var grouped = await filtered
                    .Where(e => e.Date >= start && e.Date <= end)
                    .GroupBy(e => new { e.Date.Year, e.Date.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Total = g.Sum(x => x.Amount)
                    })
                    .ToListAsync();

                
                var cursor = new DateTime(start.Year, start.Month, 1);
                var lastMonth = new DateTime(end.Year, end.Month, 1);

                while (cursor <= lastMonth)
                {
                    var match = grouped.FirstOrDefault(x =>
                        x.Year == cursor.Year && x.Month == cursor.Month);

                    trendLabels.Add($"{cursor.Year:D4}-{cursor.Month:D2}");
                    trendValues.Add(match?.Total ?? 0m);

                    cursor = cursor.AddMonths(1);
                }
            }


            // Active Saving Plan Badge
            var activePlanId = await _context.SavingPlans
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.IsActive)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

            ActiveSavingPlanBadgeVM? badge = null;

            if (activePlanId.HasValue)
            {
                var dash = await _savingPlanDashboardService.BuildAsync(activePlanId.Value, userId);
                if (dash != null)
                {
                    decimal? goalRemaining = null;
                    if (dash.GoalToDate.HasValue)
                        goalRemaining = dash.GoalToDate.Value - dash.SavingsToDate;

                    badge = new ActiveSavingPlanBadgeVM
                    {
                        PlanId = dash.Plan.Id,
                        StartDate = dash.Plan.StartDate,
                        EndDate = dash.Plan.EndDate,
                        EffectiveEnd = dash.EffectiveEnd,
                        SavingsToDate = dash.SavingsToDate,
                        GoalToDate = dash.GoalToDate,
                        GoalRemaining = goalRemaining,
                    };
                }
            }

            return new ExpensesIndexVM
            {
                Filters = filters,

                Expenses = expenses,

                TotalAmount = total,
                ExpenseCount = count,
                AverageAmount = average,

                Years = years,
                Categories = categories,

                CategorySummary = categorySummary,
                ChartLabels = categorySummary.Select(s => s.CategoryName).ToList(),
                ChartValues = categorySummary.Select(s => s.TotalAmount).ToList(),

                TrendLabels = trendLabels,
                TrendValues = trendValues,

                ActivePlan = badge,

                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
            };

            
        }
    }
}
