using Microsoft.AspNetCore.Mvc;
using GymBoom.Data;
using GymBoom.Models;
using GymBoom.Models.ViewModels.Cart;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace GymBoom.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    // --- YARDIMCI METOTLAR ---
    private List<CartItem> GetCartItems()
    {
        var cartJson = HttpContext.Session.GetString("Cart");
        if (string.IsNullOrEmpty(cartJson))
        {
            return new List<CartItem>();
        }
        return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
    }

    private void SaveCartItems(List<CartItem> cart)
    {
        var cartJson = JsonSerializer.Serialize(cart);
        HttpContext.Session.SetString("Cart", cartJson);
    }

    [HttpGet]
    public IActionResult Index()
{
    var cart = GetCartItems();
    
    var model = new CartViewModel
    {
        Items = cart.Select(c => new CartItemViewModel 
        {
            ProductId = c.ProductId,
            ProductName = c.ProductName,
            Price = c.Price,
            Quantity = c.Quantity
        }).ToList()
    };
    
    return View(model);
}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        var cart = GetCartItems();
        var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 1
            });
        }

        SaveCartItems(cart);
        TempData["SuccessMessage"] = $"{product.Name} sepete eklendi!";
        return RedirectToAction("Index", "Store");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveItem(int productId)
    {
        var cart = GetCartItems();
        var itemToRemove = cart.FirstOrDefault(c => c.ProductId == productId);

        if (itemToRemove != null)
        {
            cart.Remove(itemToRemove);
            SaveCartItems(cart);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        HttpContext.Session.Remove("Cart");
        return RedirectToAction("Index");
    }

    // --- YENİ SİPARİŞ VE ÖDEME İŞLEMLERİ ---
    [HttpGet]
    [Authorize]
    public IActionResult Checkout()
    {
        var cart = GetCartItems();
        
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
        var cart = GetCartItems();
        if (!cart.Any())
        {
            return RedirectToAction("Index", "Store");
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId)) return RedirectToAction("Login", "Account");

        var order = new Order
        {
            UserId = userId,
            OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            TotalAmount = cart.Sum(c => c.Price * c.Quantity),
            Address = address,
            City = city,
            Status = "Onaylandı",
            CreatedDate = DateTime.UtcNow,
            OrderItems = new List<OrderItem>()
        };

        foreach (var item in cart)
        {
            order.OrderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            });
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        HttpContext.Session.Remove("Cart");

        TempData["SuccessMessage"] = $"Siparişiniz başarıyla alındı! Takip Numaranız: {order.OrderNumber}";
        return RedirectToAction("Profile", "Account");
    }
}