using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GymBoom.Models;
using GymBoom.Data.Repositories;

namespace GymBoom.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrderController : Controller
{
    private readonly IRepository<Order> _orderRepository;

    public OrderController(IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orders = await _orderRepository.Query()
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
            
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string newStatus)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order != null)
        {
            order.Status = newStatus;
            _orderRepository.Update(order);
            await _orderRepository.SaveAsync();
            TempData["SuccessMessage"] = $"{order.OrderNumber} numaralı sipariş durumu '{newStatus}' olarak güncellendi!";
        }
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderRepository.Query()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        return View(order);
    }
}
