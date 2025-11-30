using Microsoft.AspNetCore.Identity;

namespace ExpenseTrackerApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Extra columns for AspNetusers table
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
