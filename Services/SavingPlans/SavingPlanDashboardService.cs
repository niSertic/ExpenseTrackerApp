using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTrackerApp.Data;
using ExpenseTrackerApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerApp.Services.SavingPlans
{
    public class SavingPlanDashboardService : ISavingPlanDashboardService
    {

        private readonly ApplicationDbContext _context;

        public SavingPlanDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SavingPlanDashboardVM?> BuildAsync(int planId, string userId)
        {
            var plan = await _context.SavingPlans
                .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

            if (plan == null) return null;

            var today = DateTime.Today;
            var effectiveEnd = plan.EndDate.Date < today ? plan.EndDate.Date : today;

            int monthsElapsed = 0;
            if (effectiveEnd >= plan.StartDate.Date)
            {
                monthsElapsed = MonthsInclusive(plan.StartDate, effectiveEnd);
            }

            var incomeToDate = monthsElapsed * plan.ExpectedMonthlyIncome;

            var spentToDate = 0m;
            if (monthsElapsed > 0)
            {
                spentToDate = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                                e.Date.Date >= plan.StartDate.Date &&
                                e.Date.Date <= effectiveEnd)
                    .SumAsync(e => (decimal?)e.Amount) ?? 0m;
            }

            var savingsToDate = incomeToDate - spentToDate;

            decimal? goalToDate = null;
            decimal? progressPercent = null;

            if (plan.PlannedMonthlySavings.HasValue)
            {
                goalToDate = monthsElapsed * plan.PlannedMonthlySavings.Value;

                if (goalToDate.Value > 0)
                {
                    progressPercent = Math.Round((savingsToDate / goalToDate.Value) * 100m, 1);
                }
            }

            // Month list for whole plan (inclusive)
            var monthStarts = new List<DateTime>();
            var cursor = new DateTime(plan.StartDate.Year, plan.StartDate.Month, 1);
            var endMonth = new DateTime(plan.EndDate.Year, plan.EndDate.Month, 1);

            while (cursor <= endMonth)
            {
                monthStarts.Add(cursor);
                cursor = cursor.AddMonths(1);
            }

            // Spent grouped by month in plan range
            var spentByMonth = await _context.Expenses
                .Where(e => e.UserId == userId &&
                            e.Date.Date >= plan.StartDate.Date &&
                            e.Date.Date <= plan.EndDate.Date)
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Spent = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            var spentLookup = spentByMonth.ToDictionary(x => (x.Year, x.Month), x => x.Spent);

            var monthlyRows = monthStarts.Select(ms =>
            {
                spentLookup.TryGetValue((ms.Year, ms.Month), out var spent);

                return new SavingPlanMonthRowVM
                {
                    Year = ms.Year,
                    Month = ms.Month,
                    Income = plan.ExpectedMonthlyIncome,
                    Spent = spent
                };
            }).ToList();

            return new SavingPlanDashboardVM
            {
                Plan = plan,
                EffectiveEnd = effectiveEnd,
                MonthsElapsed = monthsElapsed,
                IncomeToDate = incomeToDate,
                SpentToDate = spentToDate,
                SavingsToDate = savingsToDate,
                GoalToDate = goalToDate,
                ProgressPercent = progressPercent,
                MonthlyRows = monthlyRows
            };
        }

        private static int MonthsInclusive(DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;
            return ((end.Year - start.Year) * 12) + end.Month - start.Month + 1;
        }
    }
}
