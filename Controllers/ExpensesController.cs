using ExpenseTrackerApp.Data;
using ExpenseTrackerApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace ExpenseTrackerApp.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExpensesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(User)!;

        // GET: Expenses
        public async Task<IActionResult> Index(int? year, int? month, DateTime? from, DateTime? to, int? categoryId)
        {
            var userId = GetCurrentUserId();

            var query = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);

            if (year.HasValue)
            {
                query = query.Where(e => e.Date.Year == year.Value);
            }
            if (month.HasValue)
            {
                query = query.Where(e => e.Date.Month == month.Value);
            }
            if (from.HasValue)
            {
                query = query.Where(e => e.Date.Date >= from.Value.Date);
            }
            if (to.HasValue)
            {
                query = query.Where(e => e.Date.Date <= to.Value.Date);
            }
            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            var expenses = await query
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            // Total calculation
            var total = expenses.Sum(e => e.Amount);

            // Category summary
            var categorySummary = expenses
                .GroupBy(e => e.Category.Name)
                .Select(g => new CategorySummaryViewModel
                {
                    CategoryName = g.Key,
                    TotalAmount = g.Sum(e => e.Amount),
                    Percentage = total > 0 ? (g.Sum(e => e.Amount) / total) * 100 : 0
                })
                .OrderByDescending(s => s.TotalAmount)
                .ToList();

            // ValueBag for filters and total
            ViewBag.SelectedYear = year;
            ViewBag.SelectedMonth = month;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.TotalAmount = total;
            ViewBag.CategorySummary = categorySummary;
            ViewBag.SelectedCategoryId = categoryId;

            // get distinct years for filter dropdown
            var years = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Select(e => e.Date.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToListAsync();

            ViewBag.Years = years;

            var categories = await _context.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();

            ViewBag.Categories = categories;


            return View(expenses);
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // GET: Expenses/Create
        public async Task<IActionResult> Create()
        {
            var userId = GetCurrentUserId();

            var categories = await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId) // global + user-specific
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");

            return View();
        }

        // POST: Expenses/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Date,Amount,Description")] Expense expense)
        {
            var userId = GetCurrentUserId();
            expense.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", expense.CategoryId);

            return View(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", expense.CategoryId);

            return View(expense);
        }

        // POST: Expenses/Edit/5
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryId,Date,Amount,Description")] Expense expense)
        {
            var userId = GetCurrentUserId();

            if (id != expense.Id)
            {
                return NotFound();
            }

            // ensure this expense belongs to current user
            var existingExpense = await _context.Expenses
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (existingExpense == null)
            {
                return NotFound();
            }

            expense.UserId = userId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id, userId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", expense.CategoryId);

            return View(expense);
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id, string userId)
        {
            return _context.Expenses.Any(e => e.Id == id && e.UserId == userId);
        }
    }
}
