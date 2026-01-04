using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Services.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ExpenseTrackerApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHomeService _homeDashboardService;

        public HomeController(UserManager<ApplicationUser> userManager, IHomeService homeDashboardService)
        {
            _userManager = userManager;
            _homeDashboardService = homeDashboardService;

        }

        private string GetCurrentUserId() => _userManager.GetUserId(User)!;

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var vm = await _homeDashboardService.BuildAsync(userId);
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
