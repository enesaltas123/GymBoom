using Microsoft.AspNetCore.Mvc;
using GymBoom.Models;
using GymBoom.Data.Repositories;

namespace GymBoom.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IRepository<Product> _productRepository;

    public ProductsController(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productRepository.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        await _productRepository.AddAsync(product);
        await _productRepository.SaveAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Edit(int id, [FromBody] Product product)
    {
        if (id != product.Id) return BadRequest("ID eşleşmiyor.");

        try
        {
            _productRepository.Update(product);
            await _productRepository.SaveAsync();
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "Ürün güncellenirken veritabanı hatası oluştu.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();

        _productRepository.Delete(product);
        await _productRepository.SaveAsync();
        return NoContent();
    }
}