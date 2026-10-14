using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
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
    [ValidateAntiForgeryToken]
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


    [HttpGet]
    public IActionResult ForgotPassword()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

        if (user == null)
        {
            TempData["SuccessMessage"] = "Eğer sistemimizde kayıtlı bir e-posta adresi girdiyseniz, yeni şifreniz e-posta adresinize gönderilmiştir.";
            return RedirectToAction("Login");
        }

        string tempPassword = Guid.NewGuid().ToString().Substring(0, 8);
        
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
        _userRepository.Update(user);
        await _userRepository.SaveAsync();

        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("enesson3113@gmail.com", "mzew fulr jewa rurq"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("enesson3113@gmail.com", "GymBoom Destek"),
                Subject = "GymBoom - Yeni Geçici Şifreniz",
                Body = $"Merhaba {user.FullName},\n\nHesabınız için şifre sıfırlama talebi aldık.\n\nYeni geçici şifreniz: {tempPassword}\n\nLütfen bu şifre ile sisteme giriş yapın.\n\nGymBoom Ekibi",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(user.Email);
            smtpClient.Send(mailMessage);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "E-posta gönderilirken bir hata oluştu. Lütfen internet bağlantınızı kontrol edip tekrar deneyin.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Yeni şifreniz e-posta adresinize başarıyla gönderildi. Lütfen gelen kutunuzu (ve Spam klasörünü) kontrol edin.";
        return RedirectToAction("Login");
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

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId)) return RedirectToAction("Login");

        var user = await _userRepository.Query().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return RedirectToAction("Login");

        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError("CurrentPassword", "Mevcut şifreniz hatalı. Lütfen kontrol edip tekrar deneyin.");
            return View(model);
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        _userRepository.Update(user);
        await _userRepository.SaveAsync();

        TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi.";
        return RedirectToAction("Profile");
    }
}