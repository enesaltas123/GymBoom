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
    private readonly IWebHostEnvironment _webHostEnvironment; 

    public ProductController(
        IRepository<Product> productRepository, 
        IRepository<Category> categoryRepository,
        IWebHostEnvironment webHostEnvironment)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _webHostEnvironment = webHostEnvironment;
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
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "StoreImages");
                
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                product.ImageUrl = uniqueFileName;
            }

            product.CreatedDate = DateTime.UtcNow;

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
    public async Task<IActionResult> Edit(Product product, IFormFile? imageFile) // imageFile eklendi
    {
        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "StoreImages");

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                product.ImageUrl = uniqueFileName;
            }

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
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var fileName = Path.GetFileName(product.ImageUrl); 
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "StoreImages", fileName);
                
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _productRepository.Delete(product);
            await _productRepository.SaveAsync();
            TempData["SuccessMessage"] = "Ürün ve görseli sistemden başarıyla silindi!";
        }
        return RedirectToAction(nameof(Index));
    }
}