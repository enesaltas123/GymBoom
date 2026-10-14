using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using GymBoom.Models;
using GymBoom.Data.Repositories;
using GymBoom.Services.Interfaces; 

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly IRepository<Category> _categoryRepository;

    public ProductController(IProductService productService, IRepository<Category> categoryRepository)
    {
        _productService = productService;
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllProductsAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            await _productService.AddProductAsync(product, imageFile);
            TempData["SuccessMessage"] = "Ürün başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null) return NotFound();

        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            await _productService.UpdateProductAsync(product, imageFile);
            TempData["SuccessMessage"] = "Ürün başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }

        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteProductAsync(id);
        TempData["SuccessMessage"] = "Ürün ve görseli sistemden başarıyla silindi!";
        return RedirectToAction(nameof(Index));
    }
}