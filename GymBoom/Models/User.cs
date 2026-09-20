using System;
using System.Collections.Generic;

namespace GymBoom.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public List<Order> Orders { get; set; } = new();
    public List<UserSubscription> Subscriptions { get; set; } = new();
}