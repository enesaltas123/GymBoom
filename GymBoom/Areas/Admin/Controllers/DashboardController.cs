using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymBoom.Models;
using GymBoom.Data.Repositories;

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")] 
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Order> _orderRepository;

    public DashboardController(IRepository<User> userRepository, IRepository<Order> orderRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
    }

    public async Task<IActionResult> Index()
    {
        var activeUserCount = await _userRepository.Query()
        .CountAsync(u => u.MembershipEndDate.HasValue && u.MembershipEndDate > DateTime.UtcNow);
        
        var pendingOrderCount = await _orderRepository.Query().CountAsync();

        ViewBag.ActiveUserCount = activeUserCount;
        ViewBag.PendingOrderCount = pendingOrderCount;

        return View();
    }
}