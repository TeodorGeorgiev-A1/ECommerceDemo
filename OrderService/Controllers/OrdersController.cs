using Dapr.Client;
using System.Text.Json;

namespace OrderService.Controllers;

[Route("orders")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly InventoryClient _inventoryClient;
    private readonly DaprClient _daprClient;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(InventoryClient inventoryClient, ILogger<OrdersController> logger, DaprClient daprClient)
    {
        _inventoryClient = inventoryClient;
        _logger = logger;
        _daprClient = daprClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderDto order)
    {
        // 1. Check inventory via service invocation
        var stockCheck = new StockCheckRequest()
        {
            ProductId = order.ProductId,
            Quantity = order.Quantity
        };
        var result = await _daprClient.InvokeMethodAsync<StockCheckRequest, JsonElement>("inventoryservice", "inventory/stock-check", stockCheck);
        var isAvailable = result.GetProperty("available").GetBoolean();
        if (!isAvailable)
            return BadRequest("Insufficient stock");

        var orderEvent = new
        {
            Id = Guid.NewGuid().ToString(),
            order.ProductId,
            order.Quantity
        };

        // 2. Save initial order state
        await _daprClient.SaveStateAsync("statestore", orderEvent.Id, OrderStatus.Created.ToString());
        _logger.LogInformation($"State for Order: {orderEvent.Id} - {OrderStatus.Created.ToString()}");

        // 3. Publish order-created event
        await _daprClient.PublishEventAsync("pubsub", "order-created", orderEvent);

        return Ok(orderEvent);
    }

    [HttpGet("order-status/{orderId}")]
    public async Task<IActionResult> GetOrderStatus(string orderId)
    {
        var result = await _daprClient.GetStateAsync<string>("statestore", orderId);
        if (result == null)
            return NotFound();

        return Ok(new { OrderId = orderId, Status = result });
    }

    [HttpDelete("order-status/{orderId}")]
    public async Task<IActionResult> DeleteOrderStatus(string orderId)
    {
        await _daprClient.DeleteStateAsync("statestore", orderId);
        return NoContent();
    }
}
