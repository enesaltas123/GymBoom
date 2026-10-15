using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using GymBoom.Models;

namespace GymBoom.Controllers;

public class StoreController : Controller
{
    private readonly HttpClient _httpClient;

    public StoreController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5283/"); // API adresimiz
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? categoryId, string? searchString, decimal? minPrice, decimal? maxPrice, string? sortOrder)
    {
        var activeCategories = new List<Category>();
        var products = new List<Product>();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var catResponse = await _httpClient.GetAsync("api/categories");
        if (catResponse.IsSuccessStatusCode)
        {
            var catJson = await catResponse.Content.ReadAsStringAsync();
            var allCat = JsonSerializer.Deserialize<List<Category>>(catJson, options) ?? new List<Category>();
            activeCategories = allCat.Where(c => c.IsActive).ToList();
        }

        var prodResponse = await _httpClient.GetAsync("api/products");
        if (prodResponse.IsSuccessStatusCode)
        {
            var prodJson = await prodResponse.Content.ReadAsStringAsync();
            products = JsonSerializer.Deserialize<List<Product>>(prodJson, options) ?? new List<Product>();
        }

        ViewBag.Categories = activeCategories;
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
            _ => query.OrderByDescending(p => p.Id) 
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
        var response = await _httpClient.GetAsync($"api/products/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (product == null) return NotFound();
        return View(product);
    }
}