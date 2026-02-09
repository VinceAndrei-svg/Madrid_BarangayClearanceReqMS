using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Proj1.Controllers;

[Authorize]
public class ResidentController : Controller
{
    public IActionResult Profile()
    {
        return View();
    }
}