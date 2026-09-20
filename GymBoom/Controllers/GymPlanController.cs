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

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Veritabanındaki aktif üyelik paketlerini fiyata göre artan şekilde sırala
        var plans = await _context.GymPlans
            .OrderBy(p => p.Price)
            .ToListAsync();

        return View(plans);
    }
}