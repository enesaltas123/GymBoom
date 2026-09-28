using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using GymBoom.Models;
using GymBoom.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;

    public ProductController(IRepository<Product> productRepository, IRepository<Category> categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.Query()
            .Include(p => p.Category)
            .OrderByDescending(p => p.Id)
            .ToListAsync();
            
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
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            await _productRepository.AddAsync(product);
            await _productRepository.SaveAsync();
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
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
        
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            _productRepository.Update(product);
            await _productRepository.SaveAsync();
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
        var product = await _productRepository.GetByIdAsync(id);
        if (product != null)
        {
            _productRepository.Delete(product);
            await _productRepository.SaveAsync();
            TempData["SuccessMessage"] = "Ürün sistemden silindi!";
        }
        return RedirectToAction(nameof(Index));
    }
}