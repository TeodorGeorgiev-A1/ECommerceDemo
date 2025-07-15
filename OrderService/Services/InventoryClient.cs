using System.Text.Json;

namespace OrderService.Services;

public class InventoryClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public InventoryClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<InventoryResponse> UpdateStockAsync(string productId, int quantityDelta)
    {
        var request = new UpdateRequest
        {
            ProductId = productId,
            QuantityDelta = quantityDelta
        };

        // Dapr service invocation URL:
        var url = _configuration.GetValue<string>("InventoryDaprURL") ??
                  throw new Exception("Configuration value not found.");
        url += "/update";

        var response = await _httpClient.PostAsJsonAsync(url, request);

        var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<InventoryResponse>(options);
            return result ?? new InventoryResponse { Message = "Success" };
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<InventoryResponse>(options);
            return error ?? new InventoryResponse { Error = "Unknown error" };
        }
    }

    public async Task<bool> CheckStock(string productId, int quantity)
    {
        var request = new StockCheckRequest
        {
            ProductId = productId,
            Quantity = quantity
        };

        // Dapr service invocation URL:
        var url = _configuration.GetValue<string>("InventoryDaprURL") ??
                  throw new Exception("Configuration value not found.");
        url += "/stock-check";

        var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        var response = await _httpClient.PostAsJsonAsync(url, request);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<InventoryCheckResponse>(options);
            return result?.Available ?? false;
        }
        return false; // If the request fails, assume stock is not available
    }
}