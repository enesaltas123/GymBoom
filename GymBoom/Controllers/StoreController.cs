using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymBoom.Data;

namespace GymBoom.Controllers;

public class StoreController : Controller
{
    private readonly AppDbContext _context;

    public StoreController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Kategorileri ViewBag ile sayfaya yolla
        ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

        // Ürünleri yolla
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .ToListAsync();

        return View(products);
    }
}