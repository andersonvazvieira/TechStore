namespace TechStore.API.Models;

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public string Type { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public string Observation { get; set; } = string.Empty;
}