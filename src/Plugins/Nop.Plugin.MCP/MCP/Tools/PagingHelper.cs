using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;

namespace Nop.Plugin.Misc.Mcp.MCP.Tools;

/// <summary>
/// Provides shared paging logic for the MCP tools
/// </summary>
public static class PagingHelper
{
    /// <summary>
    /// Provides the effective page size and clamps it to the configured maximum
    /// </summary>
    /// <param name="mcpSettings">MCP settings</param>
    /// <param name="pageSize">Requested page size</param>
    /// <returns>The normalized page size</returns>
    public static int NormalizePageSize(McpSettings mcpSettings, int pageSize)
    {
        var maxPageSize = mcpSettings.MaxPageSize > 0 ? mcpSettings.MaxPageSize : McpDefaults.MaxPageSize;
        var defaultPageSize = mcpSettings.DefaultPageSize > 0 ? mcpSettings.DefaultPageSize : McpDefaults.DefaultPageSize;

        if (pageSize <= 0)
            return Math.Min(defaultPageSize, maxPageSize);

        return Math.Min(pageSize, maxPageSize);
    }
}