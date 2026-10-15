using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymBoom.Models;

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class GymPlanController : Controller
{
    private readonly HttpClient _httpClient;

    public GymPlanController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5283/");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("api/gymplans");
        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var plans = JsonSerializer.Deserialize<List<GymPlan>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(plans.OrderBy(p => p.DurationInMonths));
        }
        return View(new List<GymPlan>());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GymPlan plan)
    {
        if (!ModelState.IsValid) return View(plan);

        var jsonContent = new StringContent(JsonSerializer.Serialize(plan), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/gymplans", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Üyelik paketi başarıyla oluşturuldu!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "API'ye gönderilirken hata oluştu.");
        return View(plan);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var response = await _httpClient.GetAsync($"api/gymplans/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var jsonString = await response.Content.ReadAsStringAsync();
        var plan = JsonSerializer.Deserialize<GymPlan>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(plan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(GymPlan plan)
    {
        if (!ModelState.IsValid) return View(plan);

        var jsonContent = new StringContent(JsonSerializer.Serialize(plan), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"api/gymplans/{plan.Id}", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Üyelik paketi başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Güncelleme sırasında API hatası oluştu.");
        return View(plan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/gymplans/{id}");

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Üyelik paketi başarıyla silindi!";
        }
        else
        {
            TempData["ErrorMessage"] = "Silme işlemi sırasında API hatası oluştu.";
        }
        return RedirectToAction(nameof(Index));
    }
}