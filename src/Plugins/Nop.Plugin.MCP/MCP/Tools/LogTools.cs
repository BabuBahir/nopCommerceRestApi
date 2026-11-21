using System.ComponentModel;
using ModelContextProtocol.Server;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;
using Nop.Plugin.Misc.Mcp.Dto;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Mcp.MCP.Tools;

/// <summary>
/// Represents the logging-related MCP tools
/// </summary>
[McpServerToolType]
public class LogTools : BaseMcpTool
{
    #region Fields

    protected readonly ILogger _logger;

    #endregion

    #region Ctor

    public LogTools(ILogger logger,
        McpSettings mcpSettings)
        : base(mcpSettings)
    {
        _logger = logger;
    }

    #endregion

    #region Tools

    /// <summary>
    /// Reads log records
    /// </summary>
    [McpServerTool(Name = "read_logs")]
    [Description("Returns a paginated list of log records, optionally filtered by message text, log level and creation date range.")]
    public async Task<PagedResult<LogInfo>> ReadLogsAsync(
        [Description("Search keyword matched against the log message. Empty to return all records.")] string message = null,
        [Description("Only return records of this log level (10 Debug, 20 Information, 30 Warning, 40 Error, 50 Fatal). Null to skip.")] int? logLevelId = null,
        [Description("Only return records created on or after this UTC timestamp.")] DateTime? fromUtc = null,
        [Description("Only return records created on or before this UTC timestamp.")] DateTime? toUtc = null,
        [Description("Zero-based page index.")] int pageIndex = 0,
        [Description("Page size. Cannot exceed the configured maximum.")] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
        var size = NormalizePageSize(pageSize);
        LogLevel? logLevel = logLevelId is > 0 ? (LogLevel?)logLevelId.Value : null;

        var logs = await _logger.GetAllLogsAsync(
            fromUtc: fromUtc,
            toUtc: toUtc,
            message: message ?? string.Empty,
            logLevel: logLevel,
            pageIndex: pageIndex < 0 ? 0 : pageIndex,
            pageSize: size);

        var items = logs.Select(LogInfo.FromEntity).ToList();

        return new PagedResult<LogInfo>
        {
            TotalCount = logs.TotalCount,
            PageIndex = logs.PageIndex,
            PageSize = logs.PageSize,
            TotalPages = logs.TotalPages,
            Items = items
        };
    }

    #endregion
}