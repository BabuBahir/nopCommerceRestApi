using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Api.Rest.Models.Requests;

public record AddCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    public string? AttributesXml { get; set; }
}
