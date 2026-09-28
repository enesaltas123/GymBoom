using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using GymBoom.Models;

namespace GymBoom.Controllers
{
    public class ContactController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
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
                        From = new MailAddress("enesson3113@gmail.com"),
                        Subject = $"GymBoom İletişim Formu: {model.Name}",
                        Body = $"Gönderen: {model.Name}\nE-Posta: {model.Email}\n\nMesaj:\n{model.Message}",
                        IsBodyHtml = false,
                    };

                    mailMessage.To.Add("enesson3113@gmail.com"); 

                    smtpClient.Send(mailMessage);
                    ViewBag.Message = "Mesajınız başarıyla gönderildi!";
                    ModelState.Clear();
                    return View(new ContactViewModel());
                }
                catch
                {
                    ViewBag.Error = "Mesaj gönderilirken bir hata oluştu.";
                }
            }
            return View(model);
        }

    }
}