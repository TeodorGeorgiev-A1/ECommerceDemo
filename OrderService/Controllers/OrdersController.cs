using Dapr.Client;

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
        var stockCheckResult = await _inventoryClient.CheckStock(order.ProductId, order.Quantity);
        if (!stockCheckResult)
            return BadRequest("Insufficient stock");

        // 2. Publish order-created event
        var orderEvent = new
        {
            Id = Guid.NewGuid().ToString(),
            order.ProductId,
            order.Quantity
        };

        await _daprClient.PublishEventAsync("pubsub", "order-created", orderEvent);
        return Ok(orderEvent);
    }
}
