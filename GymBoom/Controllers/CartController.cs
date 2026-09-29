using Microsoft.AspNetCore.Mvc;
using GymBoom.Models;
using GymBoom.Models.ViewModels.Cart;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using GymBoom.Data.Repositories;

namespace GymBoom.Controllers;

public class CartController : Controller
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Order> _orderRepository;

    public CartController(IRepository<Product> productRepository, IRepository<Order> orderRepository)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }

    private List<CartItem> GetCartItems()
    {
        var cartJson = HttpContext.Session.GetString("Cart");
        if (string.IsNullOrEmpty(cartJson)) return new List<CartItem>();
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
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return NotFound();

        var cart = GetCartItems();
        var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

        if (existingItem != null) existingItem.Quantity++;
        else
        {
            cart.Add(new CartItem { ProductId = product.Id, ProductName = product.Name, Price = product.Price, Quantity = 1 });
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
        if (!cart.Any()) return RedirectToAction("Index", "Store");

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
            OrderItems = cart.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            }).ToList()
        };

        await _orderRepository.AddAsync(order);

        foreach (var item in cart)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.Stock -= item.Quantity;
                
                if(product.Stock < 0) product.Stock = 0;
                
                _productRepository.Update(product);
            }
        }

        await _orderRepository.SaveAsync();
        await _productRepository.SaveAsync();

        HttpContext.Session.Remove("Cart");
        TempData["SuccessMessage"] = $"Siparişiniz başarıyla alındı! Takip Numaranız: {order.OrderNumber}";
        return RedirectToAction("Profile", "Account");
    }
}