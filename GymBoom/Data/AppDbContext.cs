using Microsoft.EntityFrameworkCore;
using GymBoom.Models;

namespace GymBoom.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<GymPlan> GymPlans => Set<GymPlan>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
}