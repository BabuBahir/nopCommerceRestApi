using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a light-weight shipment projection
/// </summary>
public record ShipmentInfo
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string TrackingNumber { get; set; }

    public decimal? TotalWeight { get; set; }

    public DateTime? ShippedDateUtc { get; set; }

    public DateTime? DeliveryDateUtc { get; set; }

    public DateTime? ReadyForPickupDateUtc { get; set; }

    public string AdminComment { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public IList<ShipmentItemInfo> Items { get; set; } = new List<ShipmentItemInfo>();

    /// <summary>
    /// Creates an ShipmentInfo instance from a Shipment entity
    /// </summary>
    /// <param name="shipment">Shipment entity</param>
    /// <returns>The ShipmentInfo instance, or null when the entity is null</returns>
    public static ShipmentInfo FromEntity(Shipment shipment)
    {
        if (shipment is null)
            return null;

        return new ShipmentInfo
        {
            Id = shipment.Id,
            OrderId = shipment.OrderId,
            TrackingNumber = shipment.TrackingNumber,
            TotalWeight = shipment.TotalWeight,
            ShippedDateUtc = shipment.ShippedDateUtc,
            DeliveryDateUtc = shipment.DeliveryDateUtc,
            ReadyForPickupDateUtc = shipment.ReadyForPickupDateUtc,
            AdminComment = shipment.AdminComment,
            CreatedOnUtc = shipment.CreatedOnUtc
        };
    }
}