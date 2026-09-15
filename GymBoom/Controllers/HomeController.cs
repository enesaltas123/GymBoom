using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymBoom.Data;
using GymBoom.Models;

namespace GymBoom.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            // Tanıtım vitrini için 2 popüler plan getiren kod
            Plans = await _context.GymPlans
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.DurationInMonths)
                .Take(2)
                .ToListAsync(),

            // Tanıtım vitrini için 3 adet popüler ürün getiren kod
            Products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToListAsync()
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}