namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a light-weight order projection
/// </summary>
public record OrderInfo
{
    public int Id { get; set; }

    public string OrderNumber { get; set; }

    public Guid OrderGuid { get; set; }

    public int CustomerId { get; set; }

    public int StoreId { get; set; }

    public string OrderStatus { get; set; }

    public string PaymentStatus { get; set; }

    public string ShippingStatus { get; set; }

    public string PaymentMethodSystemName { get; set; }

    public decimal OrderSubtotalInclTax { get; set; }

    public decimal OrderShippingInclTax { get; set; }

    public decimal OrderTax { get; set; }

    public decimal OrderDiscount { get; set; }

    public decimal OrderTotal { get; set; }

    public decimal RefundedAmount { get; set; }

    public string CustomerCurrencyCode { get; set; }

    public string VatNumber { get; set; }

    public string ShippingMethod { get; set; }

    public string BillingFirstName { get; set; }

    public string BillingLastName { get; set; }

    public string BillingEmail { get; set; }

    public string BillingCity { get; set; }

    public string BillingPhoneNumber { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? PaidDateUtc { get; set; }

    public IList<OrderItemInfo> Items { get; set; } = new List<OrderItemInfo>();
}