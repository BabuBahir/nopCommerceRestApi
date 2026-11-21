using Nop.Core.Domain.Logging;

namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a light-weight log record projection
/// </summary>
public record LogInfo
{
    public int Id { get; set; }

    public string LogLevel { get; set; }

    public string ShortMessage { get; set; }

    public string FullMessage { get; set; }

    public string IpAddress { get; set; }

    public int? CustomerId { get; set; }

    public string PageUrl { get; set; }

    public string ReferrerUrl { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Creates an LogInfo instance from a Log entity
    /// </summary>
    /// <param name="log">Log entity</param>
    /// <returns>The LogInfo instance, or null when the entity is null</returns>
    public static LogInfo FromEntity(Log log)
    {
        if (log is null)
            return null;

        return new LogInfo
        {
            Id = log.Id,
            LogLevel = log.LogLevel.ToString(),
            ShortMessage = log.ShortMessage,
            FullMessage = log.FullMessage,
            IpAddress = log.IpAddress,
            CustomerId = log.CustomerId,
            PageUrl = log.PageUrl,
            ReferrerUrl = log.ReferrerUrl,
            CreatedOnUtc = log.CreatedOnUtc
        };
    }
}