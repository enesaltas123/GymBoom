using Microsoft.AspNetCore.Mvc;
using GymBoom.Models;
using GymBoom.Data.Repositories;

namespace GymBoom.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GymPlansController : ControllerBase
{
    private readonly IRepository<GymPlan> _gymPlanRepository;

    public GymPlansController(IRepository<GymPlan> gymPlanRepository)
    {
        _gymPlanRepository = gymPlanRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var plans = await _gymPlanRepository.GetAllAsync();
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var plan = await _gymPlanRepository.GetByIdAsync(id);
        if (plan == null) return NotFound();
        return Ok(plan);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GymPlan plan)
    {
        await _gymPlanRepository.AddAsync(plan);
        await _gymPlanRepository.SaveAsync();
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Edit(int id, [FromBody] GymPlan plan)
    {
        if (id != plan.Id) return BadRequest("ID eşleşmiyor.");

        try
        {
            _gymPlanRepository.Update(plan);
            await _gymPlanRepository.SaveAsync();
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "Güncelleme sırasında veritabanı hatası oluştu.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var plan = await _gymPlanRepository.GetByIdAsync(id);
        if (plan == null) return NotFound();

        _gymPlanRepository.Delete(plan);
        await _gymPlanRepository.SaveAsync();
        return NoContent();
    }
}