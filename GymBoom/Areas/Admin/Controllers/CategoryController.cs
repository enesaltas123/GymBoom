using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using GymBoom.Models;
using Microsoft.AspNetCore.Authorization;

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]

public class CategoryController : Controller
{
    private readonly HttpClient _httpClient;

    public CategoryController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5286/"); 
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("api/categories");

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var categories = JsonSerializer.Deserialize<List<Category>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(categories);
        }

        return View(new List<Category>());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category newCategory)
    {
        if (!ModelState.IsValid) return View(newCategory);

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(newCategory),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("api/categories", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Kategori API üzerinden başarıyla eklendi!";
            return RedirectToAction("Index"); 
        }

        ModelState.AddModelError(string.Empty, "API'ye gönderilirken bir hata oluştu.");
        return View(newCategory);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/categories/{id}");

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Kategori API üzerinden başarıyla silindi!";
        }
        else
        {
            TempData["ErrorMessage"] = "Kategori silinirken API tarafında bir hata oluştu.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var response = await _httpClient.PostAsync($"api/categories/{id}/toggle", null);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Kategori durumu başarıyla güncellendi!";
        }
        else
        {
            TempData["ErrorMessage"] = "Durum güncellenirken API tarafında bir hata oluştu.";
        }

        return RedirectToAction("Index");
    }
}