namespace InventoryService.Controllers;

[Route("inventory")]
[ApiController]
public class InventoryController : ControllerBase
{
    private readonly InventoryStore _store;

    public InventoryController(InventoryStore store)
    {
        _store = store;
    }

    [HttpGet("{productId}")]
    public IActionResult Get(string productId)
    {
        var item = _store.GetById(productId);
        return item is null ? NotFound("Product not found.") : Ok(item);
    }

    [HttpPost("update")]
    public IActionResult UpdateStock([FromBody] UpdateRequest request)
    {
        var result = _store.TryUpdateQuantity(request.ProductId, request.QuantityDelta);
        if (result.Success)
        {
            return Ok(new { message = "Stock updated." });
        }

        return BadRequest(new InventoryResponse() { Error = result.ErrorMessage });
    }

    [HttpPost("stock-check")]
    public IActionResult CheckStock([FromBody] StockCheckRequest request)
    {
        var product = _store.GetById(request.ProductId);

        if (product == null)
            return NotFound("Product not found");

        if (product.Quantity >= request.Quantity)
            return Ok(new { Available = true });
        else
            return Ok(new { Available = false });
    }
}
