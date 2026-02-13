using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proj1.Models;
using Proj1.Persons;

namespace Proj1.Controllers;

public class HomeController : Controller
{
    /// <summary>
    /// Smart home redirect based on user role
    /// </summary>
    public IActionResult Index()
    {
        // If user is authenticated, redirect to role-specific dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            // Admin and Staff go to Admin Dashboard
            if (User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Staff))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // Residents stay on the welcome page (or you can redirect elsewhere)
            if (User.IsInRole(Roles.Resident))
            {
                return View(); // Shows the Welcome page
            }
        }

        // Not authenticated or unknown role - show welcome page
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}