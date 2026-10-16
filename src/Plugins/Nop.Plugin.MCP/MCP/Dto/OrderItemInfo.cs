namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a light-weight order item projection
/// </summary>
public record OrderItemInfo
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPriceInclTax { get; set; }

    public decimal UnitPriceExclTax { get; set; }

    public decimal PriceInclTax { get; set; }

    public decimal PriceExclTax { get; set; }

    public decimal DiscountAmountInclTax { get; set; }

    public string AttributeDescription { get; set; }

    public decimal? ItemWeight { get; set; }

    public DateTime? RentalStartDateUtc { get; set; }

    public DateTime? RentalEndDateUtc { get; set; }
}