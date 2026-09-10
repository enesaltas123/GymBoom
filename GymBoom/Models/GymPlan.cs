using System;

namespace GymBoom.Models;

public class GymPlan : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationInMonths { get; set; }
    public string? Features { get; set; }
}