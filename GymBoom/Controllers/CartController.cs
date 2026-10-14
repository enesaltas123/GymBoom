using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GymBoom.Services.Interfaces;

namespace GymBoom.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = _cartService.GetCartViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId)
    {
        var result = await _cartService.AddToCartAsync(productId);
        
        if (result.IsSuccess) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction("Index", "Store");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IncreaseQuantity(int productId)
    {
        var result = await _cartService.IncreaseQuantityAsync(productId);
        
        if (!result.IsSuccess) TempData["ErrorMessage"] = result.Message;
            
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DecreaseQuantity(int productId)
    {
        _cartService.DecreaseQuantity(productId);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveItem(int productId)
    {
        _cartService.RemoveItem(productId);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _cartService.ClearCart();
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize]
    public IActionResult Checkout()
    {
        var cart = _cartService.GetCartItems();
        if (!cart.Any())
        {
            TempData["ErrorMessage"] = "Sepetiniz boş, ödeme yapamazsınız.";
            return RedirectToAction("Index", "Store");
        }
        return View(cart);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(string address, string city)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId)) return RedirectToAction("Login", "Account");

        var result = await _cartService.PlaceOrderAsync(address, city, userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Index");
        }

        TempData["SuccessMessage"] = $"Siparişiniz başarıyla alındı! Takip Numaranız: {result.OrderNumber}";
        return RedirectToAction("Profile", "Account");
    }
}