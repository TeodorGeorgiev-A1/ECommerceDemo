namespace InventoryService.Controllers;

[ApiController]
public class InventoryEventsController : ControllerBase
{
    private readonly InventoryStore _store;
    private readonly ILogger<InventoryEventsController> _logger;
    private static readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

    public InventoryEventsController(InventoryStore store, ILogger<InventoryEventsController> logger)
    {
        _store = store;
        _logger = logger;
    }

    [Topic("pubsub", "order-created")]
    [HttpPost("order-created")]
    public IActionResult HandleOrderCreated([FromBody] OrderDto order)
    {
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
        return Ok();
    }
}