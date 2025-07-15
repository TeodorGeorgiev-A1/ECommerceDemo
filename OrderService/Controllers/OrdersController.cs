namespace OrderService.Controllers;

[Route("orders")]
[ApiController]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateOrder([FromBody] Order order)
    {
        // In a real app, this would check inventory, charge payment, etc.
        return Ok(new { Message = $"Order {order.Id} received", order });
    }
}
