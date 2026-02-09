using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj1.Persons;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.ViewModels;

namespace Proj1.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly IAccountService _accountService;

    public AdminController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CreateStaff()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var dto = new CreateStaffDto
        {
            Email = model.Email,
            Password = model.Password
        };

        var result = await _accountService.CreateStaffAsync(dto);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }

        TempData["Success"] = "Staff account created.";
        return RedirectToAction(nameof(Dashboard));
    }
}