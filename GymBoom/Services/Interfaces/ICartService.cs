using GymBoom.Models;
using GymBoom.Models.ViewModels.Cart;

namespace GymBoom.Services.Interfaces;

public interface ICartService
{
    CartViewModel GetCartViewModel();
    List<CartItem> GetCartItems();
    Task<(bool IsSuccess, string Message)> AddToCartAsync(int productId);
    Task<(bool IsSuccess, string Message)> IncreaseQuantityAsync(int productId);
    void DecreaseQuantity(int productId);
    void RemoveItem(int productId);
    void ClearCart();
    Task<(bool IsSuccess, string Message, string OrderNumber)> PlaceOrderAsync(string address, string city, int userId);
}