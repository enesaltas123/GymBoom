using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using GymBoom.Models.ViewModels.Account;

namespace GymBoom.Controllers;

public class AccountController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AccountController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5283/");
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
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

        var jsonContent = new StringContent(JsonSerializer.Serialize(new { FullName = model.FullName, Email = model.Email, Password = model.Password }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/auth/register", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorMsg = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("Email", errorMsg);
            return View(model);
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(jsonString);
        var token = jsonDoc.RootElement.GetProperty("token").GetString();
        var userElement = jsonDoc.RootElement.GetProperty("user");

        await SignInUserLocally(userElement, token!);
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

        var jsonContent = new StringContent(JsonSerializer.Serialize(new { Email = model.Email, Password = model.Password }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/auth/login", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorMsg = await response.Content.ReadAsStringAsync();
            
            ModelState.AddModelError(string.Empty, errorMsg.Contains("askıya") ? errorMsg : "Geçersiz e-posta veya şifre girdiniz.");
            return View(model);
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(jsonString);
        var token = jsonDoc.RootElement.GetProperty("token").GetString();
        var userElement = jsonDoc.RootElement.GetProperty("user");

        await SignInUserLocally(userElement, token!);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            if (model.ReturnUrl.Contains("/Cart/", StringComparison.OrdinalIgnoreCase)) return RedirectToAction("Index", "Store"); 
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserLocally(JsonElement userElement, string jwtToken)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userElement.GetProperty("id").GetInt32().ToString()),
            new Claim(ClaimTypes.Name, userElement.GetProperty("fullName").GetString()!),
            new Claim(ClaimTypes.Email, userElement.GetProperty("email").GetString()!),
            new Claim(ClaimTypes.Role, userElement.GetProperty("role").GetString()!),
            new Claim("JwtToken", jwtToken) 
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var authProperties = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) };

        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);
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
    public IActionResult ForgotPassword() => View();
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        TempData["ErrorMessage"] = "Şifre sıfırlama işlemi yakında API üzerinden aktif edilecektir.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    [Authorize]
    public IActionResult Profile() 
    {
        TempData["ErrorMessage"] = "Profil sayfası API entegrasyonu tamamlanana kadar devre dışıdır.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword() => View();

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult ChangePassword(ChangePasswordViewModel model)
    {
        TempData["ErrorMessage"] = "Şifre değiştirme işlemi yakında API üzerinden aktif edilecektir.";
        return RedirectToAction("Index", "Home");
    }
}