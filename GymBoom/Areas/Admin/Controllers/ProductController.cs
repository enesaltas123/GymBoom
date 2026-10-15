using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using GymBoom.Models;

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(HttpClient httpClient, IWebHostEnvironment webHostEnvironment)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5283/");
        _webHostEnvironment = webHostEnvironment;
    }

    private async Task<List<Category>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetAsync("api/categories");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Category>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Category>();
        }
        return new List<Category>();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("api/products");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(products);
        }
        return View(new List<Product>());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await GetCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            product.ImageUrl = await ProcessImageAsync(imageFile, null);
            product.CreatedDate = DateTime.UtcNow;

            var jsonContent = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/products", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Ürün API üzerinden başarıyla eklendi!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, "API hatası oluştu.");
        }
        
        var categories = await GetCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var response = await _httpClient.GetAsync($"api/products/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var categories = await GetCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product?.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            product.ImageUrl = await ProcessImageAsync(imageFile, product.ImageUrl);

            var jsonContent = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/products/{product.Id}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Ürün başarıyla güncellendi!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, "Güncelleme sırasında API hatası oluştu.");
        }

        var categories = await GetCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var getResponse = await _httpClient.GetAsync($"api/products/{id}");
        if (getResponse.IsSuccessStatusCode)
        {
            var json = await getResponse.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "StoreImages", product.ImageUrl);
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
            }

            var deleteResponse = await _httpClient.DeleteAsync($"api/products/{id}");
            if (deleteResponse.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Ürün ve görseli sistemden başarıyla silindi!";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> ProcessImageAsync(IFormFile? imageFile, string? existingImageUrl)
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

            if (!string.IsNullOrEmpty(existingImageUrl))
            {
                string oldFilePath = Path.Combine(uploadsFolder, existingImageUrl);
                if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
            }

            return uniqueFileName;
        }
        return existingImageUrl; 
    }
}