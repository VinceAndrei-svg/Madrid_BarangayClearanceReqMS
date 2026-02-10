using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.ViewModels;
using Proj1.Persons;

namespace Proj1.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
public class ResidentController : Controller
{
    private readonly IResidentService _service;
    private readonly IMapper _mapper;

    public async Task<IActionResult> Index(string? search, string? sort, int? minAge, int? maxAge, int page = 1, int pageSize = 10)
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

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateResidentViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var dto = _mapper.Map<CreateResidentDto>(model);
        await _service.CreateAsync(dto);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto == null) return NotFound();

        var vm = _mapper.Map<EditResidentViewModel>(dto);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditResidentViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var dto = _mapper.Map<UpdateResidentDto>(model);
        var updated = await _service.UpdateAsync(dto);

        if (!updated) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();

        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto == null) return NotFound();

        var vm = _mapper.Map<ResidentDetailsViewModel>(dto);
        return View(vm);
    }
    
    [HttpGet]
    public async Task<IActionResult> ExportCsv(string? search, string? sort, int? minAge, int? maxAge)
    {
        var paged = await _service.GetPagedAsync(1, int.MaxValue, search, sort, minAge, maxAge);
        var residents = _mapper.Map<List<ResidentViewModel>>(paged.Items);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("FullName,Address,BirthDate");

        foreach (var r in residents)
        {
            sb.AppendLine($"\"{r.FullName}\",\"{r.Address}\",\"{r.BirthDate:yyyy-MM-dd}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "residents.csv");
    }
    
    private readonly IPdfService _pdfService;

    public ResidentController(IResidentService service, IMapper mapper, IPdfService pdfService)
    {
        _service = service;
        _mapper = mapper;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(string? search, string? sort, int? minAge, int? maxAge)
    {
        var paged = await _service.GetPagedAsync(1, int.MaxValue, search, sort, minAge, maxAge);
        var residents = _mapper.Map<List<ResidentViewModel>>(paged.Items);

        var pdfBytes = _pdfService.GenerateResidentListPdf(residents);
        return File(pdfBytes, "application/pdf", "residents.pdf");
    }
    
}