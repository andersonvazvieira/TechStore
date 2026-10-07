namespace TechStore.Mobile.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public int MinimumStock { get; set; }

    public int? CategoryId { get; set; }

    public CategoryInfo? Category { get; set; }
}

public class CategoryInfo
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}