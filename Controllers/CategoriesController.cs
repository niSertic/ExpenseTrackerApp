using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseTrackerApp.Data;
using ExpenseTrackerApp.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTrackerApp.Controllers
{
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
            var normalizedName = (category.Name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                ModelState.AddModelError(nameof(Category.Name), "Name is required.");
                return View(category);
            }

            // Check for duplicate category names for the user and global categories
            var exists = await _context.Categories.AnyAsync(c =>
                c.Name.ToLower() == normalizedName.ToLower() &&
                (c.UserId == null || c.UserId == userId));

            if (exists)
            {
                ModelState.AddModelError(nameof(Category.Name),
                    "This category name already exists.");
            }


            if (ModelState.IsValid)
            {
                category.Name = normalizedName;
                category.UserId = userId;

                _context.Add(category);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            // Ensure the category belongs to the current user
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

            // Ensure the category belongs to the current user
            var existingCategory = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (existingCategory == null)
            {
                return NotFound();
            }

            var normalizedName = (category.Name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                ModelState.AddModelError(nameof(Category.Name), "Name is required.");
                return View(category);
            }

            // Check for duplicate category names for the user and global categories
            var exists = await _context.Categories.AnyAsync(c =>
                c.Id != id &&
                c.Name.ToLower() == normalizedName.ToLower() &&
                (c.UserId == null || c.UserId == userId));

            if (exists)
            {
                ModelState.AddModelError(nameof(Category.Name),
                    "This category name already exists.");
            }


            if (ModelState.IsValid)
            {
                category.Name = normalizedName;
                category.UserId = userId;

                _context.Update(category);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            
            return View(category);
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
                .Include(c => c.User)
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
                .Include(c => c.Expenses)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category != null)
            {
                // Prevent deletion in case of existing expenses
                if (category.Expenses != null && category.Expenses.Any())
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete a category that has expenses.");
                    return View(category);
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id, string userId)
        {
            return _context.Categories.Any(c => c.Id == id && c.UserId == userId);
        }
    }
}
