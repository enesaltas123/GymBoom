using System.Collections.Generic;

namespace GymBoom.Models.ViewModels.Account;

public class ProfileViewModel
{
    public GymBoom.Models.User AppUser { get; set; } = null!;
    public List<GymBoom.Models.Order> PastOrders { get; set; } = new();
}