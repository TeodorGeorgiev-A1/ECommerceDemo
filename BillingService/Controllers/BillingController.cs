using Dapr;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace BillingService.Controllers;

[ApiController]
public class BillingController : ControllerBase
{
    private readonly ILogger<BillingController> _logger;

    public BillingController(ILogger<BillingController> logger)
    {
        _logger = logger;
    }

    [Topic("pubsub", "order-created")]
    [HttpPost("order-created")]
    public IActionResult HandleOrderCreated([FromBody] OrderDto order)
    {
        List<ValidationResult> results;

        var context = new ValidationContext(order, serviceProvider: null, items: null);
        results = new List<ValidationResult>();
        var validationResult = Validator.TryValidateObject(order, context, results, validateAllProperties: true);

        if (!validationResult)
        {
            _logger.LogWarning("Received invalid order-created event: {Errors}", string.Join(", ", results.Select(r => r.ErrorMessage)));
            return Ok(new { Errors = results.Select(r => r.ErrorMessage) });
        }

        _logger.LogInformation($"[Billing] Charging for order: {order.Id}, Product: {order.ProductId}, Quantity: {order.Quantity}");

        return Ok();
    }
}