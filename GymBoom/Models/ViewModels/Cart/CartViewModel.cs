using System.Collections.Generic;
using System.Linq;

namespace GymBoom.Models.ViewModels.Cart;

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();

    public decimal GrandTotal => Items.Sum(x => x.TotalPrice);
    public int TotalQuantity => Items.Sum(x => x.Quantity);
}