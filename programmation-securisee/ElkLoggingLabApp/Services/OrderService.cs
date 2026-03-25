using Microsoft.EntityFrameworkCore;
using ElkLoggingLabApp.Data;
using ElkLoggingLabApp.Models;

namespace ElkLoggingLabApp.Services;

public class OrderService
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext db, ILogger<OrderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all orders");
        return await _db.Orders.Include(o => o.Product).OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving order {OrderId}", id);
        return await _db.Orders.Include(o => o.Product).FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _logger.LogInformation("Creating order for product {ProductId}, quantity {Quantity}, customer {CustomerEmail}",
            order.ProductId, order.Quantity, order.CustomerEmail);

        var product = await _db.Products.FindAsync(order.ProductId);
        if (product == null)
        {
            _logger.LogError("Order creation failed: Product {ProductId} not found", order.ProductId);
            throw new InvalidOperationException($"Product {order.ProductId} not found");
        }

        order.TotalPrice = product.Price * order.Quantity;
        order.Status = "Pending";
        order.CreatedAt = DateTime.UtcNow;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Order created: {OrderId} | Product: {ProductName} | Total: {TotalPrice:C} | Customer: {CustomerEmail}",
            order.Id, product.Name, order.TotalPrice, order.CustomerEmail);

        // Simulate business rule: large orders get flagged
        if (order.TotalPrice > 1000)
        {
            _logger.LogWarning("HIGH VALUE ORDER: {OrderId} - {TotalPrice:C} for customer {CustomerEmail}",
                order.Id, order.TotalPrice, order.CustomerEmail);
        }

        return order;
    }

    public async Task<Order?> UpdateStatusAsync(int id, string status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null)
        {
            _logger.LogWarning("Order not found for status update: {OrderId}", id);
            return null;
        }

        var previousStatus = order.Status;
        order.Status = status;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Order status updated: {OrderId} | {PreviousStatus} -> {NewStatus}",
            order.Id, previousStatus, status);

        return order;
    }

    public async Task<bool> CancelAsync(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null)
        {
            _logger.LogWarning("Order not found for cancellation: {OrderId}", id);
            return false;
        }

        if (order.Status == "Completed")
        {
            _logger.LogWarning("Cannot cancel completed order: {OrderId}", id);
            return false;
        }

        order.Status = "Cancelled";
        await _db.SaveChangesAsync();

        _logger.LogInformation("Order cancelled: {OrderId} | Customer: {CustomerEmail}", id, order.CustomerEmail);
        return true;
    }
}
