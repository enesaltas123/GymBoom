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

    public async Task<IActionResult> Index(int? categoryId)
    {
        ViewBag.Categories = await _categoryRepository.GetAllAsync();
        ViewBag.CurrentCategory = categoryId; 

        var products = await _productRepository.GetAllAsync();

        if (categoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == categoryId).ToList();
        }

        return View(products);
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