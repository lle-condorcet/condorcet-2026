using Microsoft.AspNetCore.Mvc;
using ElkLoggingLabApp.Models;
using ElkLoggingLabApp.Services;

namespace ElkLoggingLabApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly AuditService _auditService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(OrderService orderService, AuditService auditService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            _logger.LogWarning("Order not found: {OrderId}", id);
            return NotFound(new { message = $"Order {id} not found" });
        }
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] Order order)
    {
        try
        {
            var created = await _orderService.CreateAsync(order);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            await _auditService.LogActionAsync("CREATE", "Order", created.Id.ToString(), "api-user",
                $"New order: Product {created.ProductId}, Qty {created.Quantity}, Total {created.TotalPrice:C}", ip);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Order creation failed");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<Order>> UpdateStatus(int id, [FromBody] StatusUpdateRequest request)
    {
        var order = await _orderService.UpdateStatusAsync(id, request.Status);
        if (order == null) return NotFound(new { message = $"Order {id} not found" });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _auditService.LogActionAsync("STATUS_UPDATE", "Order", id.ToString(), "api-user",
            $"Status changed to: {request.Status}", ip);
        return Ok(order);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _orderService.CancelAsync(id);
        if (!result) return BadRequest(new { message = $"Cannot cancel order {id}" });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _auditService.LogActionAsync("CANCEL", "Order", id.ToString(), "api-user", $"Order cancelled", ip);
        return Ok(new { message = $"Order {id} cancelled" });
    }
}

public record StatusUpdateRequest(string Status);
