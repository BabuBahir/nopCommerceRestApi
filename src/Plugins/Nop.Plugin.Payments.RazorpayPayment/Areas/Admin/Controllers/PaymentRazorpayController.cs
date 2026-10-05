using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.RazorpayPayment.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.RazorpayPayment.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class PaymentRazorpayController : BasePluginController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public PaymentRazorpayController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;

    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure()
    {
        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

        var razorpayPaymentSettings = await _settingService.LoadSettingAsync<RazorpayPaymentSettings>(storeScope);

        var model = new ConfigurationModel
        {
            KeyId = razorpayPaymentSettings.KeyId,
            KeySecret = razorpayPaymentSettings.KeySecret,
            AdditionalFee = razorpayPaymentSettings.AdditionalFee,
            AdditionalFeePercentage = razorpayPaymentSettings.AdditionalFeePercentage,
            ActiveStoreScopeConfiguration = storeScope
        };

        if (storeScope > 0)
        {
            model.KeyId_OverrideForStore = await _settingService.SettingExistsAsync(razorpayPaymentSettings, x => x.KeyId, storeScope);
            model.KeySecret_OverrideForStore = await _settingService.SettingExistsAsync(razorpayPaymentSettings, x => x.KeySecret, storeScope);
            model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(razorpayPaymentSettings, x => x.AdditionalFee, storeScope);
            model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(razorpayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
        }

        return View(model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

        var razorpayPaymentSettings = await _settingService.LoadSettingAsync<RazorpayPaymentSettings>(storeScope);

        //save settings
        razorpayPaymentSettings.KeyId = model.KeyId;
        razorpayPaymentSettings.KeySecret = model.KeySecret;
        razorpayPaymentSettings.AdditionalFee = model.AdditionalFee;
        razorpayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

        /* We do not clear cache after each setting update.
         * This behavior can increase performance, because cached settings will not be cleared 
         * and loaded from database after each update */
        await _settingService.SaveSettingOverridablePerStoreAsync(razorpayPaymentSettings, x => x.KeyId, model.KeyId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(razorpayPaymentSettings, x => x.KeySecret, model.KeySecret_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(razorpayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(razorpayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion
}