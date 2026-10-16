namespace Nop.Plugin.Misc.Mcp;

/// <summary>
/// Represents plugin constants
/// </summary>
public class McpDefaults
{
    /// <summary>
    /// Gets a plugin system name
    /// </summary>
    public static string SystemName => "Misc.Mcp";

    /// <summary>
    /// Gets the name of the authentication scheme used to guard the MCP server endpoint
    /// </summary>
    public static string ApiKeyScheme => "McpApiKey";

    /// <summary>
    /// Gets the default endpoint path of the MCP server
    /// </summary>
    public static string DefaultEndpointPath => "/mcp";

    /// <summary>
    /// Gets the name of the MCP server reported to clients
    /// </summary>
    public static string ServerName => "nopCommerce MCP";

    /// <summary>
    /// Gets the version of the MCP server reported to clients
    /// </summary>
    public static string ServerVersion => "1.0.0";

    /// <summary>
    /// Gets the MCP protocol version reported to clients
    /// </summary>
    public static string ProtocolVersion => "2025-11-25";

    /// <summary>
    /// Gets the configuration route name
    /// </summary>
    public static string ConfigurationRouteName => "Plugin.Misc.Mcp.Configure";

    /// <summary>
    /// Gets the default page size for paginated tools
    /// </summary>
    public static int DefaultPageSize => 20;

    /// <summary>
    /// Gets the maximum allowed page size for paginated tools
    /// </summary>
    public static int MaxPageSize => 100;

    /// <summary>
    /// Gets the message returned when a caller invokes a write tool while write tools are disabled
    /// </summary>
    public static string WriteToolsDisabledMessage => "Write tools are disabled on this MCP server. A store administrator must enable the \"Allow write tools\" option in the MCP plugin configuration.";
}