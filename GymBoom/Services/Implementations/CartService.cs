using GymBoom.Models;
using GymBoom.Models.ViewModels.Cart;
using GymBoom.Data.Repositories;
using GymBoom.Services.Interfaces;
using System.Text.Json;

namespace GymBoom.Services.Implementations;

public class CartService : ICartService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(
        IRepository<Product> productRepository, 
        IRepository<Order> orderRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    public List<CartItem> GetCartItems()
    {
        var cartJson = Session.GetString("Cart");
        if (string.IsNullOrEmpty(cartJson)) return new List<CartItem>();
        return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
    }

    private void SaveCartItems(List<CartItem> cart)
    {
        var cartJson = JsonSerializer.Serialize(cart);
        Session.SetString("Cart", cartJson);
    }

    public CartViewModel GetCartViewModel()
    {
        var cart = GetCartItems();
        return new CartViewModel
        {
            Items = cart.Select(c => new CartItemViewModel 
            {
                ProductId = c.ProductId,
                ProductName = c.ProductName,
                Price = c.Price,
                Quantity = c.Quantity
            }).ToList()
        };
    }

    public async Task<(bool IsSuccess, string Message)> AddToCartAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return (false, "Ürün bulunamadı.");

        var cart = GetCartItems();
        var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

        if (existingItem != null)
        {
            if (existingItem.Quantity + 1 <= product.Stock)
            {
                existingItem.Quantity++;
                SaveCartItems(cart);
                return (true, $"{product.Name} sepetinizde güncellendi!");
            }
            return (false, $"Maalesef stokta sadece {product.Stock} adet {product.Name} bulunuyor.");
        }
        else
        {
            if (product.Stock > 0)
            {
                cart.Add(new CartItem { ProductId = product.Id, ProductName = product.Name, Price = product.Price, Quantity = 1 });
                SaveCartItems(cart);
                return (true, $"{product.Name} sepete eklendi!");
            }
            return (false, $"{product.Name} ürününün stoğu tükenmiştir.");
        }
    }

    public async Task<(bool IsSuccess, string Message)> IncreaseQuantityAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return (false, "Ürün bulunamadı.");

        var cart = GetCartItems();
        var item = cart.FirstOrDefault(c => c.ProductId == productId);

        if (item != null)
        {
            if (item.Quantity + 1 <= product.Stock)
            {
                item.Quantity++;
                SaveCartItems(cart);
                return (true, "");
            }
            return (false, $"Maksimum stok sınırına ulaştınız! (Stokta kalan: {product.Stock} adet)");
        }
        return (false, "Ürün sepette bulunamadı.");
    }

    public void DecreaseQuantity(int productId)
    {
        var cart = GetCartItems();
        var item = cart.FirstOrDefault(c => c.ProductId == productId);

        if (item != null)
        {
            if (item.Quantity > 1) item.Quantity--;
            else cart.Remove(item);
            
            SaveCartItems(cart);
        }
    }

    public void RemoveItem(int productId)
    {
        var cart = GetCartItems();
        var itemToRemove = cart.FirstOrDefault(c => c.ProductId == productId);

        if (itemToRemove != null)
        {
            cart.Remove(itemToRemove);
            SaveCartItems(cart);
        }
    }

    public void ClearCart()
    {
        Session.Remove("Cart");
    }

    public async Task<(bool IsSuccess, string Message, string OrderNumber)> PlaceOrderAsync(string address, string city, int userId)
    {
        var cart = GetCartItems();
        if (!cart.Any()) return (false, "Sepetiniz boş.", "");

        foreach (var item in cart)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null || product.Stock < item.Quantity)
            {
                int currentStock = product?.Stock ?? 0;
                return (false, $"Sipariş işlemi durduruldu! Siz işlemi tamamlarken '{item.ProductName}' ürününün stoğu değişti. (Güncel Stok: {currentStock}) Lütfen sepetinizdeki adedi güncelleyin.", "");
            }
        }

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
                _productRepository.Update(product);
            }
        }

        await _orderRepository.SaveAsync();
        await _productRepository.SaveAsync();

        ClearCart();
        return (true, "Sipariş başarıyla alındı!", order.OrderNumber);
    }
}