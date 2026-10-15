using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using GymBoom.Models;

namespace GymBoom.Controllers;

public class ContactController : Controller
{
    private readonly HttpClient _httpClient;

    public ContactController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5283/");
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ContactViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(ContactViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/contact", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            ViewBag.Message = "Mesajınız başarıyla gönderildi! En kısa sürede dönüş yapacağız.";
            ModelState.Clear();
            return View(new ContactViewModel());
        }
        
        ViewBag.Error = "Mesajınız API servisine iletilemedi. Lütfen daha sonra tekrar deneyin.";
        return View(model);
    }
}