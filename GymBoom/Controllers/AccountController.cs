using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymBoom.Models;
using GymBoom.Models.ViewModels.Account;
using GymBoom.Data.Repositories;

namespace GymBoom.Controllers;

public class AccountController : Controller
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Order> _orderRepository;

    public AccountController(IRepository<User> userRepository, IRepository<Order> orderRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var existingUser = await _userRepository.Query()
            .AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());

        if (existingUser)
        {
            ModelState.AddModelError("Email", "Bu e-posta adresi ile zaten bir hesap mevcut.");
            return View(model);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

        var newUser = new User
        {
            FullName = model.FullName,
            Email = model.Email.ToLower(),
            PasswordHash = passwordHash,
            Role = "Member",
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString()),
            new Claim(ClaimTypes.Name, newUser.FullName),
            new Claim(ClaimTypes.Email, newUser.Email),
            new Claim(ClaimTypes.Role, newUser.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var authProperties = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) };

        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

[HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre girdiniz.");
            return View(model);
        }

        if (user.IsActive == false)
        {
            ModelState.AddModelError(string.Empty, "Hesabınız askıya alınmıştır. Lütfen yönetici ile iletişime geçin.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var authProperties = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) };

        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);

        // Yönlendirme işlemleri
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            if (model.ReturnUrl.Contains("/Cart/AddToCart", StringComparison.OrdinalIgnoreCase) || 
                model.ReturnUrl.Contains("/Cart/RemoveItem", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Store"); 
            }
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId)) return RedirectToAction("Login");

        var user = await _userRepository.Query()
            .Include(u => u.ActiveGymPlan)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return RedirectToAction("Login");
        
        var userOrders = await _orderRepository.Query()
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();

        var model = new ProfileViewModel
        {
            AppUser = user,
            PastOrders = userOrders
        };

        return View(model);
    }
}