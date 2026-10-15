using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GymBoom.Models;
using GymBoom.Data.Repositories;

namespace GymBoom.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IRepository<User> _userRepository;

    public UsersController(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("active-count")]
    public async Task<IActionResult> GetActiveUserCount()
    {
        var activeCount = await _userRepository.Query()
            .CountAsync(u => u.MembershipEndDate.HasValue && u.MembershipEndDate > DateTime.UtcNow);
        return Ok(activeCount);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepository.Query()
            .Include(u => u.ActiveGymPlan)
            .OrderByDescending(u => u.CreatedDate)
            .ToListAsync();

        return Ok(users);
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        if (user.Role == "Admin")
        {
            return BadRequest("Sistem yöneticisi hesabı pasife alınamaz!");
        }

        user.IsActive = !user.IsActive;
        _userRepository.Update(user);
        await _userRepository.SaveAsync();

        return Ok(new { IsActive = user.IsActive, FullName = user.FullName });
    }

    [HttpGet("my-profile")]
    [Authorize] 
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var user = await _userRepository.Query()
            .Include(u => u.ActiveGymPlan)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();

        return Ok(user);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest("Mevcut şifreniz hatalı. Lütfen kontrol edip tekrar deneyin.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        _userRepository.Update(user);
        await _userRepository.SaveAsync();

        return Ok(new { Message = "Şifreniz başarıyla güncellendi." });
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}