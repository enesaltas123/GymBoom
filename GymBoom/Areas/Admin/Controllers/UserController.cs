using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GymBoom.Models;
using GymBoom.Data.Repositories;

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly IRepository<User> _userRepository;

    public UserController(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userRepository.Query()
            .Include(u => u.ActiveGymPlan)
            .OrderByDescending(u => u.CreatedDate)
            .ToListAsync();
            
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            if (user.Role == "Admin")
            {
                TempData["ErrorMessage"] = "Sistem yöneticisi hesabı pasife alınamaz!";
                return RedirectToAction(nameof(Index));
            }
            user.IsActive = !user.IsActive;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
            
            var statusText = user.IsActive ? "aktifleştirildi" : "pasife alındı";
            TempData["SuccessMessage"] = $"{user.FullName} adlı kullanıcının hesabı {statusText}.";
        }
        return RedirectToAction(nameof(Index));
    }
}