using Microsoft.AspNetCore.Mvc;
using GymBoom.Models;
using GymBoom.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GymBoom.Controllers;

public class GymPlanController : Controller
{
    private readonly IRepository<GymPlan> _gymPlanRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserSubscription> _subscriptionRepository;

    public GymPlanController(
        IRepository<GymPlan> gymPlanRepository, 
        IRepository<User> userRepository,
        IRepository<UserSubscription> subscriptionRepository)
    {
        _gymPlanRepository = gymPlanRepository;
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var plans = await _gymPlanRepository.GetAllAsync();
        return View(plans.OrderBy(p => p.Price));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Checkout(int planId)
    {
        var selectedPlan = await _gymPlanRepository.GetByIdAsync(planId);
        if (selectedPlan == null)
        {
            return NotFound("Seçtiğiniz plan bulunamadı.");
        }

        return View(selectedPlan);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(int planId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId)) return RedirectToAction("Login", "Account");

        var selectedPlan = await _gymPlanRepository.GetByIdAsync(planId);
        if (selectedPlan == null) return NotFound("Seçilen plan bulunamadı.");

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