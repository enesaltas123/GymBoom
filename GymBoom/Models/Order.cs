using System;

namespace GymBoom.Models;

public class Order : BaseEntity
{
    public string? UserId { get; set; }
    public string CustomerFullName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; } = "Hazırlanıyor";
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}