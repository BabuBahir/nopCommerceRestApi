using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.RazorpayPayment.Areas.Admin.Models;

/// <summary>
/// Represents a configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Payments.RazorpayPayment.KeyId")]
    public string KeyId { get; set; }
    public bool KeyId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Payments.RazorpayPayment.KeySecret")]
    public string KeySecret { get; set; }
    public bool KeySecret_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Payments.RazorpayPayment.AdditionalFee")]
    public decimal AdditionalFee { get; set; }
    public bool AdditionalFee_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Payments.RazorpayPayment.AdditionalFeePercentage")]
    public bool AdditionalFeePercentage { get; set; }
    public bool AdditionalFeePercentage_OverrideForStore { get; set; }
}