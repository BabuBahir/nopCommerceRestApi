using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Mcp.Controllers;

[AutoValidateAntiforgeryToken]
[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
public class McpAdminController : BasePluginController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly ISettingService _settingService;
    protected readonly McpSettings _mcpSettings;

    #endregion

    #region Ctor

    public McpAdminController(ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        McpSettings mcpSettings)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _mcpSettings = mcpSettings;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure()
    {
        return View("~/Plugins/Misc.Mcp/Areas/Admin/Views/Configure.cshtml", _mcpSettings);
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("save")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure(McpSettings model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        _mcpSettings.Enabled = model.Enabled;
        _mcpSettings.EndpointPath = model.EndpointPath;
        _mcpSettings.ApiKey = model.ApiKey;         
        _mcpSettings.DefaultPageSize = model.DefaultPageSize;
        _mcpSettings.MaxPageSize = model.MaxPageSize;
        _mcpSettings.AllowWriteTools = model.AllowWriteTools;

        await _settingService.SaveSettingAsync(_mcpSettings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion
}