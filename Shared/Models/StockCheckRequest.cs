namespace Shared.Models;

public class StockCheckRequest
{
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
}