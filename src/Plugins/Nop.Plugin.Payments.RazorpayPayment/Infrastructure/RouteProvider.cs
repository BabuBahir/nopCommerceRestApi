using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.RazorpayPayment.Infrastructure;

/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    /// <param name="endpointRouteBuilder">Route builder</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        //Admin
        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Payments.RazorpayPayment.Admin.Configure",
            pattern: "Admin/PaymentRazorpay/Configure",
            defaults: new { controller = "PaymentRazorpay", action = "Configure", area = "Admin" });

        //Public
        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Payments.RazorpayPayment.Checkout",
            pattern: "PaymentRazorpay/Checkout",
            defaults: new { controller = "PaymentRazorpay", action = "Checkout" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Payments.RazorpayPayment.SuccessCallback",
            pattern: "PaymentRazorpay/SuccessCallback",
            defaults: new { controller = "PaymentRazorpay", action = "SuccessCallback" });
    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => 0;
}