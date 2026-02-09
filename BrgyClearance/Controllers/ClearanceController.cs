using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj1.Persons;

namespace Proj1.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
public class ClearanceController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}