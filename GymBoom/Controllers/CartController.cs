using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymBoom.Data;
using GymBoom.Extensions;
using GymBoom.Models.ViewModels.Cart;

namespace GymBoom.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly AppDbContext _context;
    private const string CartSessionKey = "GymBoomCart";

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Cart/Index
    [HttpGet]
    public IActionResult Index()
    {
        var cart = GetCart();
        return View(cart);
    }

    // POST: /Cart/AddToCart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || !product.IsActive)
        {
            return NotFound();
        }

        var cart = GetCart();
        var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            });
        }

        SaveCart(cart);

        TempData["SuccessMessage"] = $"{product.Name} sepete eklendi!";
        return RedirectToAction("Index", "Store");
    }

    // POST: /Cart/RemoveItem
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveItem(int productId)
    {
        var cart = GetCart();
        var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

        if (item != null)
        {
            cart.Items.Remove(item);
            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    // POST: /Cart/Clear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        HttpContext.Session.Remove(CartSessionKey);
        return RedirectToAction("Index");
    }

    // Yardımcı Metotlar
    private CartViewModel GetCart()
    {
        var cart = HttpContext.Session.GetObjectFromJson<CartViewModel>(CartSessionKey);
        return cart ?? new CartViewModel();
    }

    private void SaveCart(CartViewModel cart)
    {
        HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
    }
}