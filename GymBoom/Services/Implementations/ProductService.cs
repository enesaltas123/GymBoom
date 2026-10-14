using GymBoom.Models;
using GymBoom.Data.Repositories;
using GymBoom.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymBoom.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductService(IRepository<Product> productRepository, IWebHostEnvironment webHostEnvironment)
    {
        _productRepository = productRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _productRepository.Query()
            .Include(p => p.Category)
            .OrderByDescending(p => p.Id)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task AddProductAsync(Product product, IFormFile? imageFile)
    {
        product.ImageUrl = await ProcessImageAsync(imageFile, product.ImageUrl);
        product.CreatedDate = DateTime.UtcNow;
        
        await _productRepository.AddAsync(product);
        await _productRepository.SaveAsync();
    }

    public async Task UpdateProductAsync(Product product, IFormFile? imageFile)
    {
        product.ImageUrl = await ProcessImageAsync(imageFile, product.ImageUrl);
        
        _productRepository.Update(product);
        await _productRepository.SaveAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product != null)
        {
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var fileName = Path.GetFileName(product.ImageUrl);
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "StoreImages", fileName);
                
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _productRepository.Delete(product);
            await _productRepository.SaveAsync();
        }
    }

    private async Task<string?> ProcessImageAsync(IFormFile? imageFile, string? existingImageUrl)
    {
        if (imageFile != null && imageFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "StoreImages");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }
        return existingImageUrl; 
    }
}