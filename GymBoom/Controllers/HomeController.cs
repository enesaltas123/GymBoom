using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymBoom.Models;
using GymBoom.Data.Repositories;

namespace GymBoom.Controllers;

public class HomeController : Controller
{
    private readonly IRepository<GymPlan> _gymPlanRepository;
    private readonly IRepository<Product> _productRepository;

    public HomeController(IRepository<GymPlan> gymPlanRepository, IRepository<Product> productRepository)
    {
        _gymPlanRepository = gymPlanRepository;
        _productRepository = productRepository;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            Plans = await _gymPlanRepository.Query()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.DurationInMonths)
                .Take(2)
                .ToListAsync(),

            Products = await _productRepository.Query()
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