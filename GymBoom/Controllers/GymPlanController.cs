using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymBoom.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GymBoom.Controllers;

public class GymPlanController : Controller
{
    private readonly AppDbContext _context;

    public GymPlanController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var plans = await _context.GymPlans
            .OrderBy(p => p.Price)
            .ToListAsync();

        return View(plans);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Checkout(int planId)
    {
        var selectedPlan = await _context.GymPlans.FindAsync(planId);
        
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
        if (!int.TryParse(userIdString, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var selectedPlan = await _context.GymPlans.FindAsync(planId);
        if (selectedPlan == null)
        {
            return NotFound("Seçilen plan bulunamadı.");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı.");
        }

        user.ActiveGymPlanId = selectedPlan.Id;
        
        DateTime startDate = (user.MembershipEndDate.HasValue && user.MembershipEndDate.Value > DateTime.UtcNow) 
                             ? user.MembershipEndDate.Value 
                             : DateTime.UtcNow;
                             
        user.MembershipEndDate = startDate.AddMonths(selectedPlan.DurationInMonths);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{selectedPlan.Title} paketini başarıyla satın aldınız!";
        return RedirectToAction("Profile", "Account");
    }
}