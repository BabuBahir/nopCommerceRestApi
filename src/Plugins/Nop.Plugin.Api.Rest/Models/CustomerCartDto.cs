using System.Collections.Generic;

namespace Nop.Plugin.Api.Rest.Models;

/// <summary>
/// The authenticated customer's cart for the current store
/// </summary>
public record CustomerCartDto
{
    public int StoreId { get; set; }

    public int ItemCount { get; set; }

    public int TotalQuantity { get; set; }

    public List<CustomerCartItemDto> Items { get; set; } = new();
}

/// <summary>
/// A product line in the authenticated customer's cart
/// </summary>
public record CustomerCartItemDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? Sku { get; set; }

    public int Quantity { get; set; }

    public string? AttributesXml { get; set; }

    public decimal UnitPrice { get; set; }

    public bool ProductAvailable { get; set; }
}
