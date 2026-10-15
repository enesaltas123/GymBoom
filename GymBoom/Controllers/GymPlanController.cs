using Microsoft.AspNetCore.Mvc;
using GymBoom.Models;
using GymBoom.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace GymBoom.Controllers;

public class GymPlanController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserSubscription> _subscriptionRepository;

    public GymPlanController(
        HttpClient httpClient, 
        IRepository<User> userRepository,
        IRepository<UserSubscription> subscriptionRepository)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5283/");
        
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("api/gymplans");
        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var plans = JsonSerializer.Deserialize<List<GymPlan>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(plans.OrderBy(p => p.Price));
        }
        return View(new List<GymPlan>());
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Checkout(int planId)
    {
        var response = await _httpClient.GetAsync($"api/gymplans/{planId}");
        if (!response.IsSuccessStatusCode)
        {
            return NotFound("Seçtiğiniz plan bulunamadı.");
        }
        
        var jsonString = await response.Content.ReadAsStringAsync();
        var selectedPlan = JsonSerializer.Deserialize<GymPlan>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        return View(selectedPlan);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(int planId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId)) return RedirectToAction("Login", "Account");

        var response = await _httpClient.GetAsync($"api/gymplans/{planId}");
        if (!response.IsSuccessStatusCode) return NotFound("Seçilen plan bulunamadı.");
        
        var jsonString = await response.Content.ReadAsStringAsync();
        var selectedPlan = JsonSerializer.Deserialize<GymPlan>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound("Kullanıcı bulunamadı.");

        user.ActiveGymPlanId = selectedPlan.Id;
        DateTime startDate = (user.MembershipEndDate.HasValue && user.MembershipEndDate.Value > DateTime.UtcNow) 
                             ? user.MembershipEndDate.Value 
                             : DateTime.UtcNow;
                             
        user.MembershipEndDate = startDate.AddMonths(selectedPlan.DurationInMonths);
        _userRepository.Update(user);

        var newSubscription = new UserSubscription
        {
            UserId = user.Id,
            GymPlanId = selectedPlan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = user.MembershipEndDate.Value,
            IsActive = true
        };
        await _subscriptionRepository.AddAsync(newSubscription);
        await _userRepository.SaveAsync(); 

        TempData["SuccessMessage"] = $"{selectedPlan.Title} paketini başarıyla satın aldınız!";
        return RedirectToAction("Profile", "Account");
    }
}