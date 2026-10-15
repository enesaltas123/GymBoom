using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymBoom.Models;

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrderController : Controller
{
    private readonly HttpClient _httpClient;

    public OrderController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5283/");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("api/orders");
        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var orders = JsonSerializer.Deserialize<List<Order>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(orders);
        }
        return View(new List<Order>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string newStatus)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(newStatus), Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync($"api/orders/{id}/status", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Sipariş durumu başarıyla güncellendi!";
        }
        else
        {
            TempData["ErrorMessage"] = "Sipariş durumu güncellenirken API'de bir hata oluştu.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var response = await _httpClient.GetAsync($"api/orders/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var jsonString = await response.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<Order>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (order == null) return NotFound();

        return View(order);
    }
}