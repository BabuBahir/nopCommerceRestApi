using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.RazorpayPayment.Components;

[ViewComponent(Name = "Razorpay")]
public class RazorpayViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View("~/Plugins/Nop.Plugin.Payments.RazorpayPayment/Views/PaymentRazorpay/PaymentInfo.cshtml");
    }
}