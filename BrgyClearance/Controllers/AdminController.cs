using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proj1.Persons;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.ViewModels;

namespace Proj1.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
public class AdminController : Controller
{
    private readonly IAccountService _accountService;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminController(
        IAccountService accountService,
        UserManager<IdentityUser> userManager)
    {
        _accountService = accountService;
        _userManager = userManager;
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public async Task<IActionResult> Index()
    {
        var result = await _accountService.GetStaffListAsync();

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(new List<StaffMemberViewModel>());
        }

        // Map to ViewModel
        var viewModels = result.Data!.Select(dto => new StaffMemberViewModel
        {
            Id = dto.Id,
            Email = dto.Email,
            IsActive = dto.IsActive,
            LockoutEnd = dto.LockoutEnd
        }).ToList();

        return View(viewModels);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public IActionResult CreateStaff()
    {
        return View();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
    {
        if (!ModelState.IsValid) 
            return View(model);

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

        TempData["Success"] = result.SuccessMessage;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var dto = new ToggleStaffStatusDto
        {
            StaffId = id,
            CurrentUserId = _userManager.GetUserId(User)!,
            IsCurrentUserAdmin = User.IsInRole(Roles.Admin)
        };

        var result = await _accountService.ToggleStaffStatusAsync(dto);

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
        }
        else
        {
            TempData["Success"] = result.SuccessMessage;
        }

        return RedirectToAction(nameof(Index));
    }
}