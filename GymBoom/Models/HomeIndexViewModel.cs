using System;
using System.Collections.Generic;

namespace GymBoom.Models;

public class HomeIndexViewModel
{
    public List<GymPlan> Plans { get; set; } = new();
    public List<Product> Products { get; set; } = new();
}