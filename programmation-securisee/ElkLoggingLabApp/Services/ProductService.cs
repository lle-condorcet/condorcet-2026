using Microsoft.EntityFrameworkCore;
using ElkLoggingLabApp.Data;
using ElkLoggingLabApp.Models;

namespace ElkLoggingLabApp.Services;

public class ProductService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProductService> _logger;

    public ProductService(AppDbContext db, ILogger<ProductService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all products");
        return await _db.Products.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving product {ProductId}", id);
        return await _db.Products.FindAsync(id);
    }

    public async Task<List<Product>> SearchAsync(string? name, string? category, decimal? minPrice, decimal? maxPrice)
    {
        _logger.LogInformation("Searching products: Name={Name}, Category={Category}, MinPrice={MinPrice}, MaxPrice={MaxPrice}",
            name, category, minPrice, maxPrice);

        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    /// <summary>
    /// Deliberately slow query for demonstrating slow query detection in ELK.
    /// </summary>
    public async Task<List<Product>> GetSlowAsync()
    {
        _logger.LogWarning("Executing deliberately slow product query for demo purposes");

        // Simulate a slow database operation
        await Task.Delay(600);

        var products = await _db.Products
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Price)
            .ThenBy(p => p.Category)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        // Extra delay to simulate complex processing
        await Task.Delay(200);

        _logger.LogWarning("Slow product query completed with {Count} results", products.Count);
        return products;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Product created: {ProductId} - {ProductName} ({Price:C})",
            product.Id, product.Name, product.Price);

        return product;
    }

    public async Task<Product?> UpdateAsync(int id, Product updated)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found for update: {ProductId}", id);
            return null;
        }

        product.Name = updated.Name;
        product.Description = updated.Description;
        product.Price = updated.Price;
        product.Category = updated.Category;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Product updated: {ProductId} - {ProductName}", product.Id, product.Name);
        return product;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found for deletion: {ProductId}", id);
            return false;
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Product deleted: {ProductId} - {ProductName}", id, product.Name);
        return true;
    }
}
