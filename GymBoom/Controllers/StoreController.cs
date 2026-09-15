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

    public async Task<IActionResult> Index()
    {
        // Kategorileri ViewBag ile sayfadaki görsel filtre butonları için taşıyoruz
        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .ToListAsync();

        // Tüm aktif ürünleri kategorisiyle birlikte çekiyoruz
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return View(products);
    }
}