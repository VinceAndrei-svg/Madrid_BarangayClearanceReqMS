using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proj1.Interfaces;
using Proj1.Models.ViewModels;
using Proj1.Persons;

namespace Proj1.Controllers;

/// <summary>
/// Controller for managing audit logs
/// Only accessible by administrators
/// </summary>
[Authorize(Roles = Roles.Admin)]
public class AuditController : Controller
{
    private readonly IAuditLogService _auditLogService;
    private readonly UserManager<IdentityUser> _userManager;

    public AuditController(
        IAuditLogService auditLogService,
        UserManager<IdentityUser> userManager)
    {
        _auditLogService = auditLogService;
        _userManager = userManager;
    }

    /// <summary>
    /// Displays paginated audit logs with filtering options
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 20,
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var result = await _auditLogService.GetPagedLogsAsync(
            page, 
            pageSize, 
            userId, 
            entityType, 
            action, 
            startDate, 
            endDate);

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(new AuditLogListViewModel());
        }

        // Get list of staff/admin users for filter dropdown
        var adminUsers = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        var staffUsers = await _userManager.GetUsersInRoleAsync(Roles.Staff);
        var allUsers = adminUsers.Concat(staffUsers)
            .OrderBy(u => u.Email)
            .Select(u => new { u.Id, u.Email })
            .ToList();

        var viewModel = new AuditLogListViewModel
        {
            Logs = result.Data!,
            AvailableUsers = allUsers.Select(u => new UserSelectItem 
            { 
                Id = u.Id, 
                Email = u.Email ?? "Unknown" 
            }).ToList(),
            AvailableEntityTypes = new List<string> 
            { 
                "Resident", 
                "ClearanceRequest", 
                "User", 
                "ClearanceType" 
            },
            AvailableActions = new List<string> 
            { 
                "Create", 
                "Update", 
                "Delete", 
                "Approve", 
                "Reject", 
                "RecordPayment", 
                "Release",
                "Toggle",
                "Submit"
            }
        };

        return View(viewModel);
    }

    /// <summary>
    /// Displays audit logs for a specific entity
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EntityHistory(string entityType, string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
        {
            TempData["Error"] = "Invalid entity type or ID";
            return RedirectToAction(nameof(Index));
        }

        var result = await _auditLogService.GetByEntityAsync(entityType, entityId);

        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new EntityAuditHistoryViewModel
        {
            EntityType = entityType,
            EntityId = entityId,
            Logs = result.Data!
        };

        return View(viewModel);
    }

    /// <summary>
    /// API endpoint for getting recent audit logs (for dashboard widgets)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRecentLogs(int count = 10)
    {
        var result = await _auditLogService.GetRecentLogsAsync(count);

        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.ErrorMessage });
        }

        return Json(new { success = true, data = result.Data });
    }
}