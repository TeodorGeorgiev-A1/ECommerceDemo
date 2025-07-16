using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace BillingService.Controllers;

[ApiController]
public class BillingController : ControllerBase
{
    private readonly ILogger<BillingController> _logger;
    private readonly DaprClient _daprClient;

    public BillingController(ILogger<BillingController> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    [Topic("pubsub", "order-created")]
    [HttpPost("order-created")]
    public async Task<IActionResult> HandleOrderCreated([FromBody] OrderDto order)
    {
        // 1. Validate the order object
        List<ValidationResult> results;

        var context = new ValidationContext(order, serviceProvider: null, items: null);
        results = new List<ValidationResult>();
        var validationResult = Validator.TryValidateObject(order, context, results, validateAllProperties: true);

        if (!validationResult)
        {
            _logger.LogWarning("Received invalid order-created event: {Errors}", string.Join(", ", results.Select(r => r.ErrorMessage)));
            return Ok(new { Errors = results.Select(r => r.ErrorMessage) }); // Still acknowledge to avoid infinite retries
        }

        // 2. Simulate charging the customer
        _logger.LogInformation($"[Billing] Charging for order: {order.Id}, Product: {order.ProductId}, Quantity: {order.Quantity}");

        // 3. Set order state to Confirmed
        await _daprClient.SaveStateAsync("statestore", order.Id, OrderStatus.Confirmed.ToString());
        _logger.LogInformation($"State for Order: {order.Id} - {OrderStatus.Confirmed.ToString()}");
        var state = await _daprClient.GetStateAsync<string>("statestore", order.Id);
        _logger.LogInformation($"Retrieved state for Order: {order.Id} - {state}");

        // 4. Publish order-confirmed event
        await _daprClient.PublishEventAsync("pubsub", "order-confirmed", order);

        return Ok();
    }
}