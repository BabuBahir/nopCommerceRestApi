using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ModelContextProtocol.AspNetCore;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;
using Nop.Web.Framework.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Nop.Plugin.Misc.Mcp.Infrastructure;

/// <summary>
/// Represents the MCP plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    /// <param name="endpointRouteBuilder">Route builder</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        //Admin
        endpointRouteBuilder.MapControllerRoute(name: McpDefaults.ConfigurationRouteName,
            pattern: "Admin/McpAdmin/Configure",
            defaults: new { controller = "McpAdmin", action = "Configure", area = "Admin" });

        //MCP server endpoint
        var mcpSettings = endpointRouteBuilder.ServiceProvider.GetRequiredService<McpSettings>();
        if (mcpSettings.Enabled)
            endpointRouteBuilder.MapMcp(mcpSettings.EndpointPath)
                .RequireAuthorization(policy => policy
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(McpDefaults.ApiKeyScheme));
    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => 0;
}