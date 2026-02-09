using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.ViewModels;

namespace Proj1.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var dto = new RegisterResidentDto
        {
            Email = model.Email,
            Password = model.Password
        };

        var result = await _accountService.RegisterResidentAsync(dto);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }

        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var dto = new LoginDto
        {
            Email = model.Email,
            Password = model.Password,
            RememberMe = model.RememberMe
        };

        var result = await _accountService.LoginAsync(dto);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Login failed.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }
}