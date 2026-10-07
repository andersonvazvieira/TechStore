namespace TechStore.Web.Models;

public class StockMovementViewModel
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public ProductViewModel? Product { get; set; }

    public string Type { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public DateTime Date { get; set; }

    public string Observation { get; set; } = string.Empty;
}