using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymBoom.Models;
using GymBoom.Data.Repositories;

namespace GymBoom.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IRepository<Order> _orderRepository;

    public OrdersController(IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderRepository.Query()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
            
        foreach (var order in orders)
        {
            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    item.Order = null;
                }
            }
        }

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderRepository.Query()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        if (order.OrderItems != null)
        {
            foreach (var item in order.OrderItems)
            {
                item.Order = null;
            }
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Order order)
    {
        try
        {
            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    item.Product = null; 
                    item.Order = null;
                }
            }
            order.User = null;

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveAsync();
            
            return Ok(new { Message = "Sipariş başarıyla oluşturuldu", OrderId = order.Id });
        }
        catch (Exception ex)
        {
            Console.WriteLine("API KAYIT HATASI: " + ex.Message);
            return StatusCode(500, "Sipariş kaydedilirken sunucu tarafında bir hata oluştu.");
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return NotFound();

        order.Status = newStatus;
        _orderRepository.Update(order);
        await _orderRepository.SaveAsync();
        
        return NoContent();
    }
}