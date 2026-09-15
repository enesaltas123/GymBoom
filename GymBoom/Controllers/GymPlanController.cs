using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymBoom.Data;

namespace GymBoom.Controllers;

public class GymPlanController : Controller
{
    private readonly AppDbContext _context;

    public GymPlanController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /GymPlan
    public async Task<IActionResult> Index()
    {
        // Aktif tüm üyelik paketlerini ay süresine göre sıralı hale getiriyorum
        var plans = await _context.GymPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.DurationInMonths)
            .ToListAsync();

        return View(plans);
    }
}