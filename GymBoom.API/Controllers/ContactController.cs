using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using GymBoom.Models;

namespace GymBoom.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContactController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ContactController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public IActionResult SendMessage([FromBody] ContactViewModel model)
    {
        if (model == null) return BadRequest("Model boş olamaz.");

        try
        {
            var email = _configuration["EmailSettings:Email"];
            var password = _configuration["EmailSettings:Password"];

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(email, "GymBoom İletişim Sistemi"),
                Subject = $"GymBoom Yeni Mesaj: {model.Name}",
                Body = $"Gönderen: {model.Name}\nE-Posta: {model.Email}\n\nMesaj Detayı:\n{model.Message}",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(email);

            smtpClient.Send(mailMessage);
            return Ok(new { Message = "Mesaj başarıyla gönderildi." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Mesaj gönderilirken sunucu tarafında bir hata oluştu: " + ex.Message);
        }
    }
}