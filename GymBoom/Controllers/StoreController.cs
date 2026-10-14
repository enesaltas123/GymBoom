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
    public async Task<IActionResult> Index(int? categoryId, string? searchString, decimal? minPrice, decimal? maxPrice, string? sortOrder)
    {
        var allCategories = await _categoryRepository.GetAllAsync();
        var activeCategories = allCategories.Where(c => c.IsActive == true).ToList();
        ViewBag.Categories = activeCategories;

        var products = await _productRepository.GetAllAsync();
        var activeCategoryIds = activeCategories.Select(c => c.Id).ToList();
        
        var query = products.Where(p => activeCategoryIds.Contains(p.CategoryId)).AsEnumerable();

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(p => 
                p.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) || 
                (p.Description != null && p.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            );
        }

        if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

        query = sortOrder switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            _ => query.OrderByDescending(p => p.Id) // Varsayılan: En yeniler
        };

        ViewBag.CurrentCategory = categoryId;
        ViewBag.CurrentSearch = searchString;
        ViewBag.CurrentMinPrice = minPrice;
        ViewBag.CurrentMaxPrice = maxPrice;
        ViewBag.CurrentSort = sortOrder;

        return View(query.ToList());
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