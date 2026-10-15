using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")] 
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly HttpClient _httpClient;

    public DashboardController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5283/");
    }

    public async Task<IActionResult> Index()
    {
        int activeUserCount = 0;
        int pendingOrderCount = 0;

        var userResponse = await _httpClient.GetAsync("api/users/active-count");
        if (userResponse.IsSuccessStatusCode)
        {
            var userResult = await userResponse.Content.ReadAsStringAsync();
            int.TryParse(userResult, out activeUserCount);
        }

        var orderResponse = await _httpClient.GetAsync("api/orders/count");
        if (orderResponse.IsSuccessStatusCode)
        {
            var orderResult = await orderResponse.Content.ReadAsStringAsync();
            int.TryParse(orderResult, out pendingOrderCount);
        }

        ViewBag.ActiveUserCount = activeUserCount;
        ViewBag.PendingOrderCount = pendingOrderCount;

        return View();
    }
}