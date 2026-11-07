using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Api.Rest.Models.Requests;

public record UpdateCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
