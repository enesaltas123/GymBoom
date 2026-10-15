using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using GymBoom.Models;

namespace GymBoom.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _httpClient;

    public HomeController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5283/"); // API adresimiz
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            Plans = new List<GymPlan>(),
            Products = new List<Product>()
        };

        var plansResponse = await _httpClient.GetAsync("api/gymplans");
        if (plansResponse.IsSuccessStatusCode)
        {
            var plansJson = await plansResponse.Content.ReadAsStringAsync();
            var allPlans = JsonSerializer.Deserialize<List<GymPlan>>(plansJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<GymPlan>();
            model.Plans = allPlans.Where(p => p.IsActive).OrderByDescending(p => p.DurationInMonths).Take(2).ToList();
        }

        var productsResponse = await _httpClient.GetAsync("api/products");
        if (productsResponse.IsSuccessStatusCode)
        {
            var productsJson = await productsResponse.Content.ReadAsStringAsync();
            var allProducts = JsonSerializer.Deserialize<List<Product>>(productsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Product>();
            model.Products = allProducts.Where(p => p.IsActive).OrderByDescending(p => p.Id).Take(3).ToList();
        }

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}