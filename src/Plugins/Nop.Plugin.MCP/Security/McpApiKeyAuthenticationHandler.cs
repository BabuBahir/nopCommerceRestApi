using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;

namespace Nop.Plugin.Misc.Mcp.Security;

/// <summary>
/// Represents the API key authentication handler used to guard the MCP server endpoint
/// </summary>
public class McpApiKeyAuthenticationHandler : AuthenticationHandler<McpApiKeyAuthenticationOptions>
{
    #region Ctor

    public McpApiKeyAuthenticationHandler(IOptionsMonitor<McpApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handle an authentication request
    /// </summary>
    /// <returns>A task that represents the asynchronous authentication operation</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var mcpSettings = Context.RequestServices.GetService(typeof(McpSettings)) as McpSettings;
        var token = GetTokenFromRequest();

        if (string.IsNullOrWhiteSpace(token) || mcpSettings is null)
            return Task.FromResult(AuthenticateResult.NoResult());

        //1. static API key
        if (!string.IsNullOrWhiteSpace(mcpSettings.ApiKey) && KeysMatch(mcpSettings.ApiKey, token))
            return Task.FromResult(AuthenticateResult.Success(CreateTicket()));         

        return Task.FromResult(AuthenticateResult.NoResult());
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Create an authenticated ticket
    /// </summary>
    /// <returns>The authentication ticket</returns>
    protected virtual AuthenticationTicket CreateTicket()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, McpDefaults.ServerName) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationTicket(principal, Scheme.Name);
    }

    /// <summary>
    /// Get the API key from the Authorization (Bearer) or X-Api-Key request headers
    /// </summary>
    /// <returns>The API key, if present</returns>
    protected virtual string GetTokenFromRequest()
    {
        if (Request.Headers.TryGetValue("Authorization", out var authorization))
        {
            var value = authorization.ToString();
            if (value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return value["Bearer ".Length..].Trim();
        }

        if (Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
            return apiKey.ToString().Trim();

        return null;
    }

    /// <summary>
    /// Compare the expected and provided keys in constant time
    /// </summary>
    /// <param name="expected">Expected key</param>
    /// <param name="provided">Provided key</param>
    /// <returns>True if the keys match</returns>
    protected virtual bool KeysMatch(string expected, string provided)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var providedBytes = Encoding.UTF8.GetBytes(provided);

        return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    #endregion
}