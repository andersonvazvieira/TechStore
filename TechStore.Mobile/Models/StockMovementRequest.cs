namespace TechStore.Mobile.Models;

public class StockMovementRequest
{
    public int ProductId { get; set; }

    public string Type { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string Observation { get; set; } = string.Empty;
}