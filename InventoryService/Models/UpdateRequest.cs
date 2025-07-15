namespace InventoryService.Models;

public class UpdateRequest
{
    public string ProductId { get; set; } = default!;
    public int QuantityDelta { get; set; }
}