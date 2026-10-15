using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymBoom.Models;
using GymBoom.Data.Repositories;

namespace GymBoom.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IRepository<User> userRepository, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try 
        {
            var user = await _userRepository.Query()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Geçersiz e-posta veya şifre.");
            }

            if (!user.IsActive)
            {
                return BadRequest("Hesabınız askıya alınmıştır.");
            }

            var token = GenerateJwtToken(user);

            return Ok(new 
            { 
                Token = token,
                User = new { user.Id, user.FullName, user.Email, user.Role }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı girişi sırasında hata oluştu. Email: {Email}", request.Email);
            
            return StatusCode(500, "Sunucu tarafında beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try 
        {
            var existingUser = await _userRepository.Query()
                .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (existingUser) return BadRequest("Bu e-posta adresi ile zaten bir hesap mevcut.");

            var newUser = new User
            {
                FullName = request.FullName,
                Email = request.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Member",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveAsync();

            var token = GenerateJwtToken(newUser);

            return Ok(new 
            { 
                Token = token,
                User = new { newUser.Id, newUser.FullName, newUser.Email, newUser.Role }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yeni kullanıcı kaydı sırasında hata oluştu. Email: {Email}", request.Email);
            return StatusCode(500, "Kayıt işlemi sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
        }
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? "Kullanıcı"),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? "Member")
        };

        var secretKey = _configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new Exception("API appsettings.json dosyasında 'JwtSettings:SecretKey' bulunamadı!");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}