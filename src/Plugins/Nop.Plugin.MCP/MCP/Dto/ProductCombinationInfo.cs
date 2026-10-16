namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a product attribute combination projection
/// </summary>
public record ProductCombinationInfo
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string AttributesXml { get; set; }

    public int StockQuantity { get; set; }

    public bool AllowOutOfStockOrders { get; set; }

    public string Sku { get; set; }

    public string ManufacturerPartNumber { get; set; }

    public string Gtin { get; set; }

    public decimal? OverriddenPrice { get; set; }

    public int NotifyAdminForQuantityBelow { get; set; }

    public int MinStockQuantity { get; set; }
}