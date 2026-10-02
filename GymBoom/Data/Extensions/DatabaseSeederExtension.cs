using GymBoom.Data;
using GymBoom.Models;
using Microsoft.EntityFrameworkCore;

namespace GymBoom.Extensions;

public static class DatabaseSeederExtension
{
    public static void UseDatabaseSeeder(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.Database.Migrate();

        if (!dbContext.Users.Any(u => u.Role == "Admin"))
        {
            var defaultAdmin = new User
            {
                FullName = "Sistem Yöneticisi",
                Email = "admin@gymboom.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin1234"),
                Role = "Admin",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            dbContext.Users.Add(defaultAdmin);
            dbContext.SaveChanges();
        }
    }
}