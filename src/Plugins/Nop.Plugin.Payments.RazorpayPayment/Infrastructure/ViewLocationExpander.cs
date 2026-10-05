using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Payments.RazorpayPayment.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.AreaName == "Admin")
        {
            viewLocations = new[] { "/Plugins/Nop.Plugin.Payments.RazorpayPayment/Areas/Admin/Views/{1}/{0}.cshtml", "/Plugins/Nop.Plugin.Payments.RazorpayPayment/Areas/Admin/Views/Shared/{0}.cshtml" }.Concat(viewLocations);
        }
        else
        {
            viewLocations = new[] { "/Plugins/Nop.Plugin.Payments.RazorpayPayment/Views/{1}/{0}.cshtml", "/Plugins/Nop.Plugin.Payments.RazorpayPayment/Views/Shared/{0}.cshtml" }.Concat(viewLocations);
        }

        return viewLocations;
    }
}