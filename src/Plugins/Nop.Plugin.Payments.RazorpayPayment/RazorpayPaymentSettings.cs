using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.RazorpayPayment;

/// <summary>
/// Represents settings of the Razorpay payment plugin
/// </summary>
public class RazorpayPaymentSettings : ISettings
{
    /// <summary>
    /// Gets or sets the Key Id
    /// </summary>
    public string KeyId { get; set; }

    /// <summary>
    /// Gets or sets the Key Secret
    /// </summary>
    public string KeySecret { get; set; }

    /// <summary>
    /// Gets or sets an additional fee
    /// </summary>
    public decimal AdditionalFee { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
    /// </summary>
    public bool AdditionalFeePercentage { get; set; }
}