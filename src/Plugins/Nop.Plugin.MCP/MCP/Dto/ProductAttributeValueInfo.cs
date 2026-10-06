namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a product attribute value projection
/// </summary>
public record ProductAttributeValueInfo
{
    public int Id { get; set; }

    public string Name { get; set; }

    public decimal PriceAdjustment { get; set; }

    public bool PriceAdjustmentUsePercentage { get; set; }

    public decimal WeightAdjustment { get; set; }

    public decimal Cost { get; set; }

    public bool IsPreSelected { get; set; }

    public int DisplayOrder { get; set; }
}