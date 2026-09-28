using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymBoom.Models;
using GymBoom.Data.Repositories;

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class GymPlanController : Controller
{
    private readonly IRepository<GymPlan> _planRepository;

    public GymPlanController(IRepository<GymPlan> planRepository)
    {
        _planRepository = planRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var plans = await _planRepository.GetAllAsync();
        return View(plans.OrderBy(p => p.DurationInMonths));
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GymPlan plan)
    {
        if (ModelState.IsValid)
        {
            await _planRepository.AddAsync(plan);
            await _planRepository.SaveAsync();
            TempData["SuccessMessage"] = "Üyelik paketi başarıyla oluşturuldu!";
            return RedirectToAction(nameof(Index));
        }
        return View(plan);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var plan = await _planRepository.GetByIdAsync(id);
        if (plan == null) return NotFound();
        return View(plan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(GymPlan plan)
    {
        if (ModelState.IsValid)
        {
            _planRepository.Update(plan);
            await _planRepository.SaveAsync();
            TempData["SuccessMessage"] = "Üyelik paketi başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(plan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var plan = await _planRepository.GetByIdAsync(id);
        if (plan != null)
        {
            _planRepository.Delete(plan);
            await _planRepository.SaveAsync();
            TempData["SuccessMessage"] = "Üyelik paketi silindi!";
        }
        return RedirectToAction(nameof(Index));
    }
}