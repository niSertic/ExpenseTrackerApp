using ExpenseTrackerApp.Data;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Models.ViewModels;
using ExpenseTrackerApp.Services.SavingPlans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ExpenseTrackerApp.Controllers
{
    [Authorize]
    public class SavingPlansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISavingPlanService _dashboardService;

        public SavingPlansController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ISavingPlanService dashboardService)
        {
            _context = context;
            _userManager = userManager;
            _dashboardService = dashboardService;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(User)!;

        // GET: SavingPlans
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            var plans = await _context.SavingPlans
                            .Where(p => p.UserId == userId)
                            .OrderByDescending(p => p.StartDate)
                            .ToListAsync();

            return View(plans);
        }

        // GET: SavingPlans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var vm = await _dashboardService.BuildAsync(id.Value, userId);
            if (vm == null) return NotFound();

            return View(vm);
        }

        // GET: SavingPlans/Create
        public IActionResult Create()
        {
            var today = DateTime.Today;
            return View(new SavingPlan
            {
                StartDate = new DateTime(today.Year, today.Month, 1),
                EndDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1)
            });
        }

        // POST: SavingPlans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartDate,EndDate,ExpectedMonthlyIncome,PlannedMonthlySavings,Notes")] SavingPlan plan)
        {
            var UserId = GetCurrentUserId();
            plan.UserId = UserId;

            if (plan.EndDate.Date < plan.StartDate.Date)
            {
                ModelState.AddModelError(nameof(SavingPlan.EndDate), "Wrong entry - end date before start date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(plan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            return View(plan);
        }

        // GET: SavingPlans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var plan = await _context.SavingPlans.
                FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            
            if (plan == null)
            {
                return NotFound();
            }
            
            return View(plan);
        }

        // POST: SavingPlans/Edit/5
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartDate,EndDate,ExpectedMonthlyIncome,PlannedMonthlySavings,Notes")] SavingPlan plan)
        {
            var userId = GetCurrentUserId();

            if (id != plan.Id)
            {
                return NotFound();
            }

            var existing = await _context.SavingPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (existing == null) return NotFound();

            plan.UserId = userId;

            if(plan.EndDate.Date < plan.StartDate.Date)
            {
                ModelState.AddModelError(nameof(SavingPlan.EndDate), "Wrong entry - end date before start date.");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SavingPlanExists(plan.Id, userId))
                    {
                        return NotFound();
                   
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            return View(plan);
        }

        // GET: SavingPlans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var plan = await _context.SavingPlans
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            
            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        // POST: SavingPlans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            var plan = await _context.SavingPlans
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (plan != null)
            {
                _context.SavingPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive(int id)
        {
            var userId = GetCurrentUserId();

            var plan = await _context.SavingPlans
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (plan == null) return NotFound();

            // Make all user's plans inactive first 
            var userPlans = await _context.SavingPlans
                .Where(p => p.UserId == userId && p.IsActive)
                .ToListAsync();

            foreach (var p in userPlans)
                p.IsActive = false;

            // Activate selected plan
            plan.IsActive = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearActive()
        {
            var userId = GetCurrentUserId();

            var activePlans = await _context.SavingPlans
                .Where(p => p.UserId == userId && p.IsActive)
                .ToListAsync();

            foreach (var p in activePlans)
                p.IsActive = false;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SavingPlanExists(int id, string userId)
        {
            return _context.SavingPlans.Any(p => p.Id == id && p.UserId == userId);
        }
    }
}
