    using System.Security.Claims;
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
        public async Task<IActionResult> Index()
        {
            var requests = await _service.GetAllAsync();
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
                TempData["Error"] = "Resident profile not found.";
                return RedirectToAction("Index", "Home");
            }

            var requests = await _service.GetByResidentIdAsync(resident.Id);
            var vm = _mapper.Map<List<ClearanceRequestViewModel>>(requests);
            return View(vm);
        }

        [Authorize(Roles = "Resident")]
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

            await _service.CreateAsync(dto);
            
            TempData["Success"] = "Your clearance request has been submitted successfully!";
            return RedirectToAction(nameof(MyRequests));
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
    }