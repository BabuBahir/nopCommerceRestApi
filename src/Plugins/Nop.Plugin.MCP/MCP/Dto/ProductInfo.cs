namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a light-weight product projection
/// </summary>
public record ProductInfo
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Sku { get; set; }

    public string ShortDescription { get; set; }

    public decimal Price { get; set; }

    public decimal OldPrice { get; set; }

    public int StockQuantity { get; set; }

    public string ProductType { get; set; }

    public bool Published { get; set; }

    public string SeName { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    public IList<string> Categories { get; set; } = new List<string>();

    public IList<string> Manufacturers { get; set; } = new List<string>();
}