using GymBoom.Models;
using GymBoom.Models.ViewModels.Cart;
using GymBoom.Services.Interfaces;
using System.Text.Json;
using System.Text;

namespace GymBoom.Services.Implementations;

public class CartService : ICartService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public CartService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5283/");
        _httpContextAccessor = httpContextAccessor;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
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
        var response = await _httpClient.GetAsync($"api/products/{productId}");
        if (!response.IsSuccessStatusCode) return (false, "Ürün bulunamadı.");
        
        var product = JsonSerializer.Deserialize<Product>(await response.Content.ReadAsStringAsync(), _jsonOptions);
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
        var response = await _httpClient.GetAsync($"api/products/{productId}");
        if (!response.IsSuccessStatusCode) return (false, "Ürün bulunamadı.");
        
        var product = JsonSerializer.Deserialize<Product>(await response.Content.ReadAsStringAsync(), _jsonOptions);
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
            var response = await _httpClient.GetAsync($"api/products/{item.ProductId}");
            if (!response.IsSuccessStatusCode) return (false, "Ürünlerden biri bulunamadı.", "");
            
            var product = JsonSerializer.Deserialize<Product>(await response.Content.ReadAsStringAsync(), _jsonOptions);
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

        var orderJsonContent = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");
        var orderResponse = await _httpClient.PostAsync("api/orders", orderJsonContent);

        if (!orderResponse.IsSuccessStatusCode)
        {
            return (false, "Sipariş oluşturulurken sunucu tarafında bir hata meydana geldi.", "");
        }

        foreach (var item in cart)
        {
            var prodResponse = await _httpClient.GetAsync($"api/products/{item.ProductId}");
            if (prodResponse.IsSuccessStatusCode)
            {
                var product = JsonSerializer.Deserialize<Product>(await prodResponse.Content.ReadAsStringAsync(), _jsonOptions);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    var updateJson = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");
                    await _httpClient.PutAsync($"api/products/{product.Id}", updateJson);
                }
            }
        }

        ClearCart();
        return (true, "Sipariş başarıyla alındı!", order.OrderNumber);
    }
}