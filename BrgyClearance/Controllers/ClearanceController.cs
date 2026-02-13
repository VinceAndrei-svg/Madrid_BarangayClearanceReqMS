using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.ViewModels;
using Proj1.Models.Common.Enums;

namespace Proj1.Controllers;

[Authorize]
public class ClearanceController : Controller
{
    private readonly IClearanceRequestService _service;
    private readonly IClearanceTypeService _typeService;
    private readonly IResidentService _residentService;
    private readonly IMapper _mapper;

    public ClearanceController(
        IClearanceRequestService service, 
        IClearanceTypeService typeService,
        IResidentService residentService,
        IMapper mapper)
    {
        _service = service;
        _typeService = typeService;
        _residentService = residentService;
        _mapper = mapper;
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Index(string? status)
    {
        List<ClearanceRequestDto> requests;
        
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<RequestStatus>(status, out var requestStatus))
        {
            requests = await _service.GetByStatusAsync(requestStatus);
            ViewBag.CurrentStatus = status;
        }
        else
        {
            requests = await _service.GetAllAsync();
        }
        
        var vm = _mapper.Map<List<ClearanceRequestViewModel>>(requests);
        return View(vm);
    }

    [Authorize(Roles = "Resident")]
    public async Task<IActionResult> MyRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var resident = await _residentService.GetByUserIdAsync(userId);
        if (resident == null)
        {
            TempData["Error"] = "Resident profile not found. Please contact support.";
            return RedirectToAction("Index", "Home");
        }

        var requests = await _service.GetByResidentIdAsync(resident.Id);
        var vm = _mapper.Map<List<ClearanceRequestViewModel>>(requests);
        return View(vm);
    }

    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound();

        if (User.IsInRole("Resident"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var resident = await _residentService.GetByUserIdAsync(userId!);
            
            if (resident == null || request.ResidentId != resident.Id)
            {
                return Forbid();
            }
        }

        var vm = _mapper.Map<ClearanceRequestDetailsViewModel>(request);
        return View(vm);
    }

    [Authorize(Roles = "Resident")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreateClearanceRequestViewModel
        {
            ClearanceTypes = await _typeService.GetActiveAsync()
        };
        
        return View(model);
    }

    [Authorize(Roles = "Resident")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateClearanceRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ClearanceTypes = await _typeService.GetActiveAsync();
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var resident = await _residentService.GetByUserIdAsync(userId);
        if (resident == null)
        {
            ModelState.AddModelError("", "Resident profile not found. Please contact support.");
            model.ClearanceTypes = await _typeService.GetActiveAsync();
            return View(model);
        }

        var dto = new CreateClearanceRequestDto
        {
            ResidentId = resident.Id,
            ClearanceTypeId = model.ClearanceTypeId,
            Purpose = model.Purpose
        };

        var requestId = await _service.CreateAsync(dto);
        TempData["Success"] = "Your clearance request has been submitted successfully!";
        return RedirectToAction(nameof(Details), new { id = requestId });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet]
    public async Task<IActionResult> Approve(int id)
    {
        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound();

        if (!CanBeProcessed(request.Status))
        {
            TempData["Error"] = "This request cannot be approved in its current status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var vm = new ProcessClearanceRequestViewModel 
        { 
            Id = id, 
            Approve = true,
            ReferenceNumber = request.ReferenceNumber,
            ResidentName = request.ResidentFullName,
            Purpose = request.Purpose
        };
        
        return View(vm);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(ProcessClearanceRequestViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var dto = new ProcessClearanceRequestDto
        {
            Id = model.Id,
            Approve = true,
            Remarks = model.Remarks,
            ProcessedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
        };

        try
        {
            await _service.ProcessAsync(dto);
            TempData["Success"] = "Clearance request approved successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet]
    public async Task<IActionResult> Reject(int id)
    {
        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound();

        if (!CanBeProcessed(request.Status))
        {
            TempData["Error"] = "This request cannot be rejected in its current status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var vm = new ProcessClearanceRequestViewModel 
        { 
            Id = id, 
            Approve = false,
            ReferenceNumber = request.ReferenceNumber,
            ResidentName = request.ResidentFullName,
            Purpose = request.Purpose
        };
        
        return View(vm);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(ProcessClearanceRequestViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var dto = new ProcessClearanceRequestDto
        {
            Id = model.Id,
            Approve = false,
            Remarks = model.Remarks,
            ProcessedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
        };

        try
        {
            await _service.ProcessAsync(dto);
            TempData["Success"] = "Clearance request rejected.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(int id)
    {
        var staffUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (staffUserId == null) return Unauthorized();

        var success = await _service.RecordPaymentAsync(id, staffUserId);
        
        TempData[success ? "Success" : "Error"] = success
            ? "Payment recorded successfully. Request is now ready for release."
            : "Unable to record payment. Check if request is in Approved status.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsReleased(int id)
    {
        var staffUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (staffUserId == null) return Unauthorized();

        var success = await _service.MarkAsReleasedAsync(id, staffUserId);
        
        TempData[success ? "Success" : "Error"] = success
            ? "Clearance marked as released. Valid for 6 months."
            : "Unable to mark as released. Check if payment has been recorded.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Resident")]
    [HttpGet]
    public async Task<IActionResult> Cancel(int id)
    {
        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var resident = await _residentService.GetByUserIdAsync(userId!);
        
        if (resident == null || request.ResidentId != resident.Id)
        {
            return Forbid();
        }

        if (!CanBeCancelled(request.Status))
        {
            TempData["Error"] = "This request cannot be cancelled in its current status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var vm = new CancelRequestViewModel
        {
            RequestId = id,
            ReferenceNumber = request.ReferenceNumber ?? "N/A"
        };

        return View(vm);
    }

    [Authorize(Roles = "Resident")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(CancelRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var success = await _service.CancelAsync(model.RequestId, userId, model.Reason);
        
        if (!success)
        {
            TempData["Error"] = "Unable to cancel request. It may have already been processed.";
            return RedirectToAction(nameof(Details), new { id = model.RequestId });
        }

        TempData["Success"] = "Request cancelled successfully.";
        return RedirectToAction(nameof(MyRequests));
    }

    private bool CanBeProcessed(RequestStatus status)
    {
        return status == RequestStatus.Submitted || status == RequestStatus.Pending;
    }

    private bool CanBeCancelled(RequestStatus status)
    {
        return status == RequestStatus.Submitted || status == RequestStatus.Pending;
    }
}