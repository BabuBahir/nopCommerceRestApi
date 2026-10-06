using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.Mcp;

/// <summary>
/// Represents the MCP (Model Context Protocol) plugin
/// </summary>
public class McpPlugin : BasePlugin, IMiscPlugin
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public McpPlugin(ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/McpAdmin/Configure";
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //settings
        await _settingService.SaveSettingAsync(new McpSettings
        {
            Enabled = true,
            EndpointPath = McpDefaults.DefaultEndpointPath,
            DefaultPageSize = McpDefaults.DefaultPageSize,
            MaxPageSize = McpDefaults.MaxPageSize,
            AllowWriteTools = false
        });

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.Mcp.Fields.Enabled"] = "Enabled",
            ["Plugins.Misc.Mcp.Fields.Enabled.Hint"] = "Check to expose the MCP server. The endpoint only becomes active after the application restarts.",
            ["Plugins.Misc.Mcp.Fields.EndpointPath"] = "Endpoint path",
            ["Plugins.Misc.Mcp.Fields.EndpointPath.Hint"] = "The URL path of the MCP server. The default value is \"/mcp\".",
            ["Plugins.Misc.Mcp.Fields.ApiKey"] = "API key",
            ["Plugins.Misc.Mcp.Fields.ApiKey.Hint"] = "A secret that MCP clients must send in the \"Authorization: Bearer\" or \"X-Api-Key\" request header. Anonymous access is never allowed; while the API key is empty the endpoint rejects all calls.",             
            ["Plugins.Misc.Mcp.Fields.DefaultPageSize"] = "Default page size",
            ["Plugins.Misc.Mcp.Fields.DefaultPageSize.Hint"] = "The number of records returned by default when a tool is called without a page size.",
            ["Plugins.Misc.Mcp.Fields.MaxPageSize"] = "Maximum page size",
            ["Plugins.Misc.Mcp.Fields.MaxPageSize.Hint"] = "The maximum number of records a single tool call may return.",
            ["Plugins.Misc.Mcp.Fields.AllowWriteTools"] = "Allow write tools",
            ["Plugins.Misc.Mcp.Fields.AllowWriteTools.Hint"] = "Check to enable the write tools (e.g. mark_order_as_paid, cancel_order, create_shipment, create_product, update_product, set_inventory, manage_tags, create_customer, update_customer, add_customer_address, set_metafield). When unchecked the MCP endpoint is strictly read-only and every write tool returns an error.",
            ["Plugins.Misc.Mcp.SecurityWarning"] = "The MCP endpoint is protected. Requests must present a valid static API key (configured below), sent in the \"Authorization: Bearer\" or \"X-Api-Key\" header. Anonymous requests are rejected with HTTP 401. By default the endpoint is read-only; write tools only work after the \"Allow write tools\" option below is enabled. The endpoint only becomes active after the application restarts."
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<McpSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.Mcp");

        await base.UninstallAsync();
    }

    #endregion
}