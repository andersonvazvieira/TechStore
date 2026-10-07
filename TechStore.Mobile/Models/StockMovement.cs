namespace TechStore.Mobile.Models;

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public string Type { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public DateTime Date { get; set; }

    public string Observation { get; set; } = string.Empty;

    public string ProductName =>
        Product?.Name ?? "Produto";

    public string FormattedDate =>
        Date.ToLocalTime()
            .ToString("dd/MM/yyyy HH:mm");
}