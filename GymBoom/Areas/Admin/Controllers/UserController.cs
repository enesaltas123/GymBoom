using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymBoom.Models;

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly HttpClient _httpClient;

    public UserController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5283/");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("api/users");
        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<User>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(users);
        }
        return View(new List<User>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var response = await _httpClient.PatchAsync($"api/users/{id}/toggle-status", null);

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            bool isActive = result.GetProperty("isActive").GetBoolean();
            string fullName = result.GetProperty("fullName").GetString() ?? "Kullanıcı";

            var statusText = isActive ? "aktifleştirildi" : "pasife alındı";
            TempData["SuccessMessage"] = $"{fullName} adlı kullanıcının hesabı {statusText}.";
        }
        else
        {
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync();
            }
            else
            {
                TempData["ErrorMessage"] = "İşlem sırasında sunucu tarafında bir hata oluştu.";
            }
        }
        return RedirectToAction(nameof(Index));
    }
}