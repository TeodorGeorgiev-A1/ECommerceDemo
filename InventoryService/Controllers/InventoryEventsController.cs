using System.Text.Json;

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

    [HttpPost("order-created")]
    public IActionResult HandleOrderCreated([FromBody] JsonElement message)
    {
        var orderJson = message.GetProperty("data").GetRawText();
        var order = JsonSerializer.Deserialize<OrderDto>(orderJson, options);

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
            return NotFound();

        if (product.Quantity < order.Quantity)
            return BadRequest("Inventory underflow");

        product.Quantity -= order.Quantity;

        return Ok();
    }

    [HttpGet("dapr/subscribe")]
    public IActionResult GetSubscriptions()
    {
        return Ok(new[] {
            new { pubsubname = "pubsub", topic = "order-created", route = "order-created" }
        });
    }
}