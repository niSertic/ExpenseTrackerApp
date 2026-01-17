using ExpenseTrackerApp.Data;
using ExpenseTrackerApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerApp.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(User)!;

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            var categories = await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }


        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Category category)
        {
            var userId = GetCurrentUserId();
            var normalizedName = NormalizeName(category.Name);

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                ModelState.AddModelError(nameof(Category.Name), "Name is required.");
                return View(category);
            }

            
            var exists = await CategoryNameExistsAsync(userId, normalizedName);

            if (exists)
            {
                ModelState.AddModelError(nameof(Category.Name),
                    "This category name already exists.");
            }


            if (!ModelState.IsValid)
            {
                return View(category);
            }

            category.Name = normalizedName;
            category.UserId = userId;

            _context.Add(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
 
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);


            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
        {
             var userId = GetCurrentUserId();

            if (id != category.Id)
            {
                return NotFound();
            }

            var existingCategory = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (existingCategory == null)
            {
                return NotFound();
            }

            var normalizedName = NormalizeName(category.Name);

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                ModelState.AddModelError(nameof(Category.Name), "Name is required.");
                return View(category);
            }

            var exists = await CategoryNameExistsAsync(userId, normalizedName, excludeCategoryId: id);

            if (exists)
            {
                ModelState.AddModelError(nameof(Category.Name),
                    "This category name already exists.");
            }


            if (!ModelState.IsValid)
            {
                return View(category);
            }

            category.Name = normalizedName;
            category.UserId = userId;

            _context.Update(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
            
            
            
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
                return NotFound();

            var hasExpenses = await _context.Expenses
                .AnyAsync(e => e.UserId == userId && e.CategoryId == id);

            if (hasExpenses)
            {
                ModelState.AddModelError(string.Empty, "Cannot delete a category that has expenses.");
                return View("Delete", category); 
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private static string NormalizeName(string? name) => (name ?? "").Trim();

        private async Task<bool> CategoryNameExistsAsync(string userId, string normalizedName, int? excludeCategoryId = null)
        {
            var lowered = normalizedName.ToLower();

            return await _context.Categories.AnyAsync(c =>
                (excludeCategoryId == null || c.Id != excludeCategoryId.Value) &&
                c.Name.ToLower() == lowered &&
                (c.UserId == null || c.UserId == userId));
        }

    }
}
