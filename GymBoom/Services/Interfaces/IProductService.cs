using GymBoom.Models;
using Microsoft.AspNetCore.Http; 

namespace GymBoom.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task AddProductAsync(Product product, IFormFile? imageFile);
    Task UpdateProductAsync(Product product, IFormFile? imageFile);
    Task DeleteProductAsync(int id);
}