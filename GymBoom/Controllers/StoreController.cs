using Microsoft.AspNetCore.Mvc;
using GymBoom.Models;
using GymBoom.Data.Repositories;

namespace GymBoom.Controllers;

public class StoreController : Controller
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository; 

    public StoreController(IRepository<Product> productRepository, IRepository<Category> categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? categoryId)
    {
        var allCategories = await _categoryRepository.GetAllAsync();
        
        var activeCategories = allCategories.Where(c => c.IsActive == true).ToList();
        ViewBag.Categories = activeCategories;

        var products = await _productRepository.GetAllAsync();

        var activeCategoryIds = activeCategories.Select(c => c.Id).ToList();
        var visibleProducts = products.Where(p => activeCategoryIds.Contains(p.CategoryId)).ToList();

        if (categoryId.HasValue)
        {
            visibleProducts = visibleProducts.Where(p => p.CategoryId == categoryId.Value).ToList();
            ViewBag.CurrentCategory = categoryId.Value;
        }
        else
        {
            ViewBag.CurrentCategory = null;
        }

        return View(visibleProducts);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }
}