using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.RazorpayPayment.Models;

public record CheckoutModel : BaseNopModel
{
    public string KeyId { get; set; }
    public string RazorpayOrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Contact { get; set; }
    public string CallbackUrl { get; set; }
}