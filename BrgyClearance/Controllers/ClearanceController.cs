using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.ViewModels;

namespace Proj1.Controllers;

[Authorize]
public class ClearanceController : Controller
{
    private readonly IClearanceRequestService _service;
    private readonly IMapper _mapper;
    private readonly IPdfService _pdfService;
    private readonly IClearanceTypeService _typeService;

    public ClearanceController(IClearanceRequestService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Index()
    {
        var requests = await _service.GetAllAsync();
        var vm = _mapper.Map<List<ClearanceRequestViewModel>>(requests);
        return View(vm);
    }

    [Authorize(Roles = "Resident")]
    public IActionResult Create()
    {
        return View(new CreateClearanceRequestViewModel());
    }

    [Authorize(Roles = "Resident")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateClearanceRequestViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var dto = new CreateClearanceRequestDto
        {
            ResidentId = model.ResidentId,
            ClearanceTypeId = model.ClearanceTypeId,
            Purpose = model.Purpose
        };

        await _service.CreateAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Approve(int id)
    {
        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound();

        return View(new ProcessClearanceRequestViewModel { Id = id, Approve = true });
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
            ProcessedByUserId = User.Identity?.Name ?? string.Empty
        };

        await _service.ProcessAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Reject(int id)
    {
        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound();

        return View(new ProcessClearanceRequestViewModel { Id = id, Approve = false });
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
            ProcessedByUserId = User.Identity?.Name ?? string.Empty
        };

        await _service.ProcessAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    public ClearanceController(IClearanceRequestService service, IClearanceTypeService typeService, IMapper mapper)
    {
        _service = service;
        _typeService = typeService;
        _mapper = mapper;
    }
}