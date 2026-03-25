using Microsoft.AspNetCore.Mvc;
using ElkLoggingLabApp.Models;
using ElkLoggingLabApp.Services;

namespace ElkLoggingLabApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly AuditService _auditService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductService productService, AuditService auditService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            return NotFound(new { message = $"Product {id} not found" });
        }
        return Ok(product);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<Product>>> Search(
        [FromQuery] string? name,
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice)
    {
        var products = await _productService.SearchAsync(name, category, minPrice, maxPrice);
        return Ok(products);
    }

    [HttpGet("slow")]
    public async Task<ActionResult<List<Product>>> GetSlow()
    {
        _logger.LogWarning("Slow endpoint invoked deliberately for demo");
        var products = await _productService.GetSlowAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] Product product)
    {
        var created = await _productService.CreateAsync(product);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _auditService.LogActionAsync("CREATE", "Product", created.Id.ToString(), "api-user", $"Created product: {created.Name}", ip);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Product>> Update(int id, [FromBody] Product product)
    {
        var updated = await _productService.UpdateAsync(id, product);
        if (updated == null) return NotFound(new { message = $"Product {id} not found" });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _auditService.LogActionAsync("UPDATE", "Product", id.ToString(), "api-user", $"Updated product: {updated.Name}", ip);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);
        if (!result) return NotFound(new { message = $"Product {id} not found" });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _auditService.LogActionAsync("DELETE", "Product", id.ToString(), "api-user", $"Deleted product {id}", ip);
        return NoContent();
    }
}
