namespace OrderService.Controllers;

[Route("orders")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly InventoryClient _inventoryClient;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(InventoryClient inventoryClient, ILogger<OrdersController> logger)
    {
        _inventoryClient = inventoryClient;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderDto order)
    {
        // Try to reserve stock in inventory
        var inventoryResult = await _inventoryClient.UpdateStockAsync(order.ProductId, -order.Quantity);

        if (!string.IsNullOrEmpty(inventoryResult.Error))
        {
            return BadRequest(inventoryResult.Error);
        }

        // Process order normally
        _logger.LogInformation($"Order accepted for product with Id: {order.ProductId}, quantity: {order.Quantity}");

        return Ok(new { status = "Order received", order });
    }
}
