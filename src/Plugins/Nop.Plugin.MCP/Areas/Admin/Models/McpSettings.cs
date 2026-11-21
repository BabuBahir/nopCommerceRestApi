using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Mcp.Areas.Admin.Models;

/// <summary>
/// Represents the MCP plugin settings
/// </summary>
public class McpSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the MCP server is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the endpoint path of the MCP server
    /// </summary>
    public string EndpointPath { get; set; } = McpDefaults.DefaultEndpointPath;

    /// <summary>
    /// Gets or sets the API key required to call the MCP server endpoints. When empty, access is blocked.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;     
    /// Gets or sets the default page size for paginated tools
    /// </summary>
    public int DefaultPageSize { get; set; } = McpDefaults.DefaultPageSize;

    /// <summary>
    /// Gets or sets the maximum allowed page size for paginated tools
    /// </summary>
    public int MaxPageSize { get; set; } = McpDefaults.MaxPageSize;

    /// <summary>
    /// Gets or sets a value indicating whether write tools (order, product, customer and metafield modifications) are enabled.
    /// When disabled the endpoint is strictly read-only.
    /// </summary>
    public bool AllowWriteTools { get; set; }
}