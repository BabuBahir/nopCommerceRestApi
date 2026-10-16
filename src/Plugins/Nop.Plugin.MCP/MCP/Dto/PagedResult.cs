namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a paginated result of the MCP tools
/// </summary>
/// <typeparam name="T">Type of the result items</typeparam>
public record PagedResult<T>
{
    public int TotalCount { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }

    public IList<T> Items { get; set; }
}