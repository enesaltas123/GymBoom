using System;

namespace GymBoom.Models;

public class UserSubscription : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }
    public string CustomerFullName { get; set; } = string.Empty;
    public int GymPlanId { get; set; }
    public GymPlan? GymPlan { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
}
