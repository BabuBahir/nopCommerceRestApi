using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;

namespace Nop.Plugin.Misc.Mcp.MCP.Tools;

/// <summary>
/// Represents a base class for the MCP tools
/// </summary>
public abstract class BaseMcpTool
{
    #region Fields

    protected readonly McpSettings _mcpSettings;

    #endregion

    #region Ctor

    protected BaseMcpTool(McpSettings mcpSettings)
    {
        _mcpSettings = mcpSettings;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Provides the effective page size and clamps it to the configured maximum
    /// </summary>
    protected int NormalizePageSize(int pageSize)
    {
        return PagingHelper.NormalizePageSize(_mcpSettings, pageSize);
    }

    /// <summary>
    /// Throws when write tools are disabled in the plugin settings
    /// </summary>
    protected void EnsureWriteToolsEnabled()
    {
        if (!_mcpSettings.AllowWriteTools)
            throw new InvalidOperationException(McpDefaults.WriteToolsDisabledMessage);
    }

    #endregion
}