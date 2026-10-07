namespace TechStore.Mobile.Models;

public class DashboardResponse
{
    public int TotalProducts { get; set; }

    public int TotalCategories { get; set; }

    public int LowStockProducts { get; set; }

    public int TotalStock { get; set; }

    public List<RecentMovement> RecentMovements { get; set; } = [];
}

public class RecentMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public DateTime Date { get; set; }

    public string Observation { get; set; } = string.Empty;
}