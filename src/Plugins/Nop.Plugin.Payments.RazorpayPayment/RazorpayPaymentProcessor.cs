using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Helpers;
using Nop.Plugin.Payments.RazorpayPayment.Components;
using Nop.Services.Common;

namespace Nop.Plugin.Payments.RazorpayPayment;

/// <summary>
/// Razorpay payment processor
/// </summary>
public class RazorpayPaymentProcessor : BasePlugin, IPaymentMethod
{
    #region Fields

    protected readonly RazorpayPaymentSettings _razorpayPaymentSettings;
    protected readonly ILocalizationService _localizationService;
    protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWorkContext _workContext;
    private readonly IGenericAttributeService _genericAttributeService;

    #endregion

    #region Ctor

    public RazorpayPaymentProcessor(RazorpayPaymentSettings razorpayPaymentSettings,
        ILocalizationService localizationService,
        IOrderTotalCalculationService orderTotalCalculationService,
        ISettingService settingService,
        IWebHelper webHelper,
        IHttpContextAccessor httpContextAccessor,
        IWorkContext workContext,
        IGenericAttributeService genericAttributeService)
    {
        _razorpayPaymentSettings = razorpayPaymentSettings;
        _localizationService = localizationService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _httpContextAccessor = httpContextAccessor;
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
    }

    #endregion

    #region Methods

    public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        return Task.FromResult(new ProcessPaymentResult());
    }

    public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        await _genericAttributeService.SaveAttributeAsync(customer, "RazorpayPaymentOrderId", postProcessPaymentRequest.Order.Id);

        _httpContextAccessor.HttpContext.Response.Redirect($"{_webHelper.GetStoreLocation()}PaymentRazorpay/Checkout");
    }


    public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        return Task.FromResult(false);
    }

    public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
            _razorpayPaymentSettings.AdditionalFee, _razorpayPaymentSettings.AdditionalFeePercentage);
    }

    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
    }

    public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
    }

    public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
    }

    public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
    }

    public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
    {
        return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
    }

    public Task<bool> CanRePostProcessPaymentAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        //we allow the customer to re-post process payment if they didn't complete it
        return Task.FromResult(true);
    }

    public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        return Task.FromResult<IList<string>>(new List<string>());
    }

    public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        return Task.FromResult(new ProcessPaymentRequest());
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PaymentRazorpay/Configure";
    }

    public Type GetPublicViewComponent()
    {
        return typeof(RazorpayViewComponent);
    }

    public override async Task InstallAsync()
    {
        //settings
        var settings = new RazorpayPaymentSettings
        {
            KeyId = "",
            KeySecret = "",
            AdditionalFee = 0,
            AdditionalFeePercentage = false
        };
        await _settingService.SaveSettingAsync(settings);

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Payments.RazorpayPayment.KeyId"] = "Key ID",
            ["Plugins.Payments.RazorpayPayment.KeyId.Hint"] = "Enter your Razorpay Key ID.",
            ["Plugins.Payments.RazorpayPayment.KeySecret"] = "Key Secret",
            ["Plugins.Payments.RazorpayPayment.KeySecret.Hint"] = "Enter your Razorpay Key Secret.",
            ["Plugins.Payments.RazorpayPayment.AdditionalFee"] = "Additional fee",
            ["Plugins.Payments.RazorpayPayment.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
            ["Plugins.Payments.RazorpayPayment.AdditionalFeePercentage"] = "Additional fee. Use percentage",
            ["Plugins.Payments.RazorpayPayment.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
            ["Plugins.Payments.RazorpayPayment.PaymentMethodDescription"] = "Pay securely using Razorpay (Credit/Debit Card, NetBanking, UPI, Wallets)"
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<RazorpayPaymentSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.RazorpayPayment");

        await base.UninstallAsync();
    }

    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payments.RazorpayPayment.PaymentMethodDescription");
    }

    #endregion

    #region Properties

    public bool SupportCapture => false;

    public bool SupportPartiallyRefund => false;

    public bool SupportRefund => false;

    public bool SupportVoid => false;

    public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

    // We use redirection so that the post process payment will redirect to our custom action
    public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

    public bool SkipPaymentInfo => true;

    #endregion
}