namespace InventoryService.Models;

public class UpdateResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public UpdateResult(bool success, string? errorMessage = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }
    public UpdateResult() : this(true)
    {
    }
}
