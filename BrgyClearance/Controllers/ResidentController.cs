using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.ViewModels;
using Proj1.Persons;

namespace Proj1.Controllers;

/// <summary>
/// Controller for managing residents (Admin only).
/// Best Practice: Clear separation of concerns with service layer.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public class ResidentController : Controller
{
    private readonly IResidentService _service;
    private readonly IMapper _mapper;
    private readonly IPdfService _pdfService;
    private readonly ILogger<ResidentController> _logger;

    public ResidentController(
        IResidentService service, 
        IMapper mapper, 
        IPdfService pdfService,
        ILogger<ResidentController> logger)
    {
        _service = service;
        _mapper = mapper;
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>
    /// Displays paginated, searchable list of residents.
    /// Best Practice: Flexible filtering with sensible defaults.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search, 
        string? sort, 
        int? minAge, 
        int? maxAge, 
        int page = 1, 
        int pageSize = 10)
    {
        try
        {
            var paged = await _service.GetPagedAsync(page, pageSize, search, sort, minAge, maxAge);
            var residents = _mapper.Map<List<ResidentViewModel>>(paged.Items);

            var vm = new ResidentIndexViewModel
            {
                Search = search,
                Residents = residents,
                Page = paged.Page,
                TotalPages = paged.TotalPages,
                PageSize = paged.PageSize,
                Sort = sort,
                MinAge = minAge,
                MaxAge = maxAge
            };

            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resident index");
            TempData["Error"] = "An error occurred while loading residents.";
            return View(new ResidentIndexViewModel());
        }
    }

    /// <summary>
    /// Displays form to create a new resident.
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Processes resident creation form.
    /// Best Practice: Validate, map, delegate to service, redirect with feedback.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateResidentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var dto = _mapper.Map<CreateResidentDto>(model);
            await _service.CreateAsync(dto);

            TempData["Success"] = "Resident created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resident");
            ModelState.AddModelError("", "An error occurred while creating the resident.");
            return View(model);
        }
    }

    /// <summary>
    /// Displays form to edit an existing resident.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                TempData["Error"] = "Resident not found.";
                return NotFound();
            }

            var vm = _mapper.Map<EditResidentViewModel>(dto);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resident {ResidentId} for edit", id);
            TempData["Error"] = "An error occurred while loading the resident.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Processes resident update form.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditResidentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var dto = _mapper.Map<UpdateResidentDto>(model);
            var updated = await _service.UpdateAsync(dto);

            if (!updated)
            {
                TempData["Error"] = "Resident not found.";
                return NotFound();
            }

            TempData["Success"] = "Resident updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resident {ResidentId}", model.Id);
            ModelState.AddModelError("", "An error occurred while updating the resident.");
            return View(model);
        }
    }

    /// <summary>
    /// Soft-deletes a resident.
    /// Best Practice: Use POST for delete operations, not GET.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                TempData["Error"] = "Unable to delete resident. Resident not found.";
                return NotFound();
            }

            TempData["Success"] = "Resident deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resident {ResidentId}", id);
            TempData["Error"] = "An error occurred while deleting the resident.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Displays detailed information about a resident.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                TempData["Error"] = "Resident not found.";
                return NotFound();
            }

            var vm = _mapper.Map<ResidentDetailsViewModel>(dto);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resident details for {ResidentId}", id);
            TempData["Error"] = "An error occurred while loading resident details.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Exports residents to CSV format.
    /// Best Practice: Use same filters as Index view for consistency.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportCsv(
        string? search, 
        string? sort, 
        int? minAge, 
        int? maxAge)
    {
        try
        {
            // Get all residents matching filters
            var paged = await _service.GetPagedAsync(1, int.MaxValue, search, sort, minAge, maxAge);
            var residents = _mapper.Map<List<ResidentViewModel>>(paged.Items);

            // Build CSV content
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Full Name,Address,Birth Date,Age");

            foreach (var r in residents)
            {
                var age = CalculateAge(r.BirthDate);
                sb.AppendLine($"\"{r.FullName}\",\"{r.Address}\",\"{r.BirthDate:yyyy-MM-dd}\",{age}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"residents_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting residents to CSV");
            TempData["Error"] = "An error occurred while exporting to CSV.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Exports residents to PDF format.
    /// Best Practice: Use dedicated PDF service for document generation.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportPdf(
        string? search, 
        string? sort, 
        int? minAge, 
        int? maxAge)
    {
        try
        {
            // Get all residents matching filters
            var paged = await _service.GetPagedAsync(1, int.MaxValue, search, sort, minAge, maxAge);
            var residents = _mapper.Map<List<ResidentViewModel>>(paged.Items);

            // Generate PDF using dedicated service
            var pdfBytes = _pdfService.GenerateResidentListPdf(residents);
            var fileName = $"residents_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting residents to PDF");
            TempData["Error"] = "An error occurred while exporting to PDF.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    /// <summary>
    /// Calculates age from birth date.
    /// Best Practice: Reusable helper method for consistent age calculation.
    /// </summary>
    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }
}