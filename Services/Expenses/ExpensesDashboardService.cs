using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ExpenseTrackerApp.Data;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerApp.Services.Expenses
{
    public class ExpensesDashboardService : IExpensesDashboardService
    {
        private readonly ApplicationDbContext _context;

        public ExpensesDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ExpensesIndexVM> BuildIndexAsync(string userId, ExpenseFiltersVM filters)
        {
            
            var query = _context.Expenses
                .AsNoTracking()
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);

            if (filters.Year.HasValue)
                query = query.Where(e => e.Date.Year == filters.Year.Value);

            if (filters.Month.HasValue)
                query = query.Where(e => e.Date.Month == filters.Month.Value);

            if (filters.From.HasValue)
                query = query.Where(e => e.Date.Date >= filters.From.Value.Date);

            if (filters.To.HasValue)
                query = query.Where(e => e.Date.Date <= filters.To.Value.Date);

            if (filters.CategoryId.HasValue)
                query = query.Where(e => e.CategoryId == filters.CategoryId.Value);

            
            var expenses = await query
                .OrderByDescending(e => e.Date)
                .ToListAsync();

           
            var total = expenses.Sum(e => e.Amount);
            var count = expenses.Count;
            var average = count > 0 ? expenses.Average(e => e.Amount) : 0m;

            
            var categorySummary = new List<CategorySummaryVM>();
            if (!filters.CategoryId.HasValue && expenses.Any())
            {
                categorySummary = expenses
                    .GroupBy(e => e.Category.Name)
                    .Select(g => new CategorySummaryVM
                    {
                        CategoryName = g.Key,
                        TotalAmount = g.Sum(x => x.Amount),
                        Percentage = total > 0 ? (g.Sum(x => x.Amount) / total) * 100m : 0m
                    })
                    .OrderByDescending(s => s.TotalAmount)
                    .ToList();
            }

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
                ChartValues = categorySummary.Select(s => s.TotalAmount).ToList()
            };
        }
    }
}
