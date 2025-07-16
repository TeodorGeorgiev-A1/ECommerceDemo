using Dapr.Client;
using Shared.Enums;

namespace InventoryService.Controllers;

[ApiController]
public class InventoryEventsController : ControllerBase
{
    private readonly InventoryStore _store;
    private readonly ILogger<InventoryEventsController> _logger;
    private readonly DaprClient _daprClient;

    public InventoryEventsController(InventoryStore store, ILogger<InventoryEventsController> logger, DaprClient daprClient)
    {
        _store = store;
        _logger = logger;
        _daprClient = daprClient;
    }

    [Topic("pubsub", "order-confirmed")]
    [HttpPost("order-confirmed")]
    public async Task<IActionResult> HandleOrderConfirmed([FromBody] OrderDto order)
    {
        // 1. Validate the order object
        if (order is null)
        {
            _logger.LogWarning("Received invalid order-created event: Empty order.");
            return Ok(); // Still acknowledge to avoid infinite retries
        }

        if (string.IsNullOrEmpty(order.ProductId))
        {
            _logger.LogWarning("Received invalid order-created event: ProductId missing.");
            return Ok(); // Still acknowledge to avoid infinite retries
        }

        // 2. Check inventory and update stock
        var product = _store.GetById(order.ProductId);

        if (product == null)
        {
            _logger.LogWarning($"Product {order.ProductId}");
            return Ok(); // Still acknowledge to avoid infinite retries
        }

        if (product.Quantity < order.Quantity)
        {
            _logger.LogWarning("Inventory underflow");
            return Ok(); // Still acknowledge to avoid infinite retries
        }

        product.Quantity -= order.Quantity;

        _logger.LogInformation($"Order processed: {order.ProductId} - {order.Quantity} units. Remaining stock: {product.Quantity}");

        // 3. Set order state to Confirmed
        await _daprClient.SaveStateAsync("statestore", order.Id, OrderStatus.Shipped.ToString());
        _logger.LogInformation($"State for Order: {order.Id} - {OrderStatus.Shipped.ToString()}");
        var state = await _daprClient.GetStateAsync<string>("statestore", order.Id);
        _logger.LogInformation($"Retrieved state for Order: {order.Id} - {state}");

        return Ok();
    }
}