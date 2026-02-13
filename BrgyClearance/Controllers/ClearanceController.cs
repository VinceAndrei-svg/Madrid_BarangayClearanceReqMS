using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.ViewModels;
using Proj1.Models.Common.Enums;

namespace Proj1.Controllers;

/// <summary>
/// Controller for managing clearance requests.
/// Best Practice: Role-based authorization with proper separation of concerns.
/// </summary>
[Authorize]
public class ClearanceController : Controller
{
    private readonly IClearanceRequestService _service;
    private readonly IClearanceTypeService _typeService;
    private readonly IResidentService _residentService;
    private readonly IPdfClearanceService _pdfService;
    private readonly IMapper _mapper;
    private readonly ILogger<ClearanceController> _logger;
    private readonly IWebHostEnvironment _environment;

    public ClearanceController(
        IClearanceRequestService service, 
        IClearanceTypeService typeService,
        IResidentService residentService,
        IPdfClearanceService pdfService,
        IMapper mapper,
        ILogger<ClearanceController> logger,
        IWebHostEnvironment environment)
    {
        _service = service;
        _typeService = typeService;
        _residentService = residentService;
        _pdfService = pdfService;
        _mapper = mapper;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Displays all clearance requests (Admin/Staff only).
    /// Best Practice: Optional filtering by status.
    /// </summary>
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

    /// <summary>
    /// Displays clearance requests for the logged-in resident.
    /// Best Practice: Automatically filters by current user's resident profile.
    /// </summary>
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

    /// <summary>
    /// Displays detailed information about a specific clearance request.
    /// Best Practice: Residents can only view their own requests.
    /// </summary>
    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound();

        // Best Practice: Authorization - Residents can only view their own requests
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

    /// <summary>
    /// Downloads the PDF clearance document.
    /// Best Practice: Only Released clearances can be downloaded.
    /// Authorization: Residents can download their own, Staff/Admin can download any.
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        try
        {
            // Get the clearance request
            var request = await _service.GetByIdAsync(id);
            if (request == null)
            {
                _logger.LogWarning("Clearance request {RequestId} not found for PDF download", id);
                TempData["Error"] = "Clearance request not found.";
                return RedirectToAction(nameof(MyRequests));
            }

            // Best Practice: Authorization - Residents can only download their own clearances
            if (User.IsInRole("Resident"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var resident = await _residentService.GetByUserIdAsync(userId!);
                
                if (resident == null || request.ResidentId != resident.Id)
                {
                    _logger.LogWarning("Unauthorized PDF download attempt by user {UserId} for request {RequestId}", 
                        userId, id);
                    return Forbid();
                }
            }

            // Best Practice: Business rule - Only Released clearances can be downloaded
            if (request.Status != RequestStatus.Released)
            {
                _logger.LogWarning("Attempted to download PDF for non-released clearance {RequestId} with status {Status}", 
                    id, request.Status);
                TempData["Error"] = "PDF is only available for released clearances.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Generate PDF
            var result = await _pdfService.GenerateClearancePdfAsync(id);
            
            if (!result.Succeeded || result.Data == null)
            {
                _logger.LogError("Failed to generate PDF for clearance {RequestId}: {Error}", 
                    id, result.ErrorMessage);
                TempData["Error"] = "Failed to generate PDF. Please try again or contact support.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Best Practice: Descriptive filename with reference number and date
            var fileName = $"Clearance_{request.ReferenceNumber}_{DateTime.Now:yyyyMMdd}.pdf";
            
            // Best Practice: Convert relative path to absolute path using WebRootPath
            var fullPath = Path.Combine(_environment.WebRootPath, result.Data.TrimStart('/'));
            
            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogError("PDF file not found at path: {FilePath}", fullPath);
                TempData["Error"] = "PDF file not found. Please try regenerating the clearance.";
                return RedirectToAction(nameof(Details), new { id });
            }
            
            _logger.LogInformation("PDF downloaded successfully for clearance {RequestId} by user {UserId}", 
                id, User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Return PDF file for download
            return File(
                System.IO.File.ReadAllBytes(fullPath), 
                "application/pdf", 
                fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading PDF for clearance request {RequestId}", id);
            TempData["Error"] = "An error occurred while downloading the PDF. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Displays form to create a new clearance request (Resident only).
    /// </summary>
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

    /// <summary>
    /// Processes new clearance request creation.
    /// Best Practice: Validates resident profile exists before creating request.
    /// </summary>
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

    /// <summary>
    /// Displays approval form for a clearance request (Staff/Admin only).
    /// </summary>
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

    /// <summary>
    /// Processes clearance request approval.
    /// Best Practice: Records which staff member processed the request.
    /// </summary>
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

    /// <summary>
    /// Displays rejection form for a clearance request (Staff/Admin only).
    /// </summary>
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

    /// <summary>
    /// Processes clearance request rejection.
    /// </summary>
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

    /// <summary>
    /// Records payment for a clearance request (Staff/Admin only).
    /// Best Practice: Single action to record payment, advances status automatically.
    /// </summary>
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

    /// <summary>
    /// Marks a clearance as released to the resident (Staff/Admin only).
    /// Best Practice: Generates PDF automatically upon release.
    /// </summary>
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

    /// <summary>
    /// Displays cancellation form for a clearance request (Resident only).
    /// </summary>
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

    /// <summary>
    /// Processes clearance request cancellation by resident.
    /// </summary>
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

    // ========================================
    // HELPER METHODS
    // ========================================

    /// <summary>
    /// Determines if a request can be processed (approved/rejected).
    /// Best Practice: Centralized business logic for status transitions.
    /// </summary>
    private bool CanBeProcessed(RequestStatus status)
    {
        return status == RequestStatus.Submitted || status == RequestStatus.Pending;
    }

    /// <summary>
    /// Determines if a request can be cancelled by the resident.
    /// </summary>
    private bool CanBeCancelled(RequestStatus status)
    {
        return status == RequestStatus.Submitted || status == RequestStatus.Pending;
    }
}