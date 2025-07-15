namespace InventoryService.Services;

public class InventoryStore
{
    private readonly Dictionary<string, InventoryItem> _items = new();

    public InventoryStore()
    {
        // Seed some items
        _items["p100"] = new InventoryItem { ProductId = "p100", ProductName = "Widget", Quantity = 10 };
        _items["p200"] = new InventoryItem { ProductId = "p200", ProductName = "Gadget", Quantity = 5 };
    }

    public InventoryItem? GetById(string productId)
        => _items.TryGetValue(productId, out var item) ? item : null;

    public UpdateResult TryUpdateQuantity(string productId, int delta)
    {
        if (!_items.TryGetValue(productId, out InventoryItem? item))
        {
            return new UpdateResult
            {
                Success = false,
                ErrorMessage = "Product not found."
            };
        }

        if (delta < 0 && item.Quantity + delta < 0)
        {
            return new UpdateResult
            {
                Success = false,
                ErrorMessage = "Not enough stock."
            };
        }

        item.Quantity += delta;
        return new();
    }

    public IEnumerable<InventoryItem> GetAll() => _items.Values;
}