using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Mcp.MCP.Tools;
using Nop.Plugin.Misc.Mcp.Security;

namespace Nop.Plugin.Misc.Mcp.Infrastructure;

/// <summary>
/// Represents the MCP plugin startup configuration
/// </summary>
public class PluginNopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        //register the MCP server with the Streamable HTTP transport (stateless mode)
        services.AddMcpServer(options =>
        {
            options.ServerInfo = new()
            {
                Name = McpDefaults.ServerName,
                Version = McpDefaults.ServerVersion
            };
        }).WithHttpTransport()
            .WithTools<CatalogTools>()
            .WithTools<OrderTools>()
            .WithTools<CustomerTools>()
            .WithTools<ProductTools>()
            .WithTools<LogTools>()
            .WithTools<MetafieldTools>();

        //register the API key authentication scheme used to guard the MCP endpoint
        services.AddAuthentication().AddScheme<McpApiKeyAuthenticationOptions, McpApiKeyAuthenticationHandler>(McpDefaults.ApiKeyScheme, options => { });
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 2000;
}