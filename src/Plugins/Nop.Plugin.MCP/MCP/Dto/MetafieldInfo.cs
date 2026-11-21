namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a metafield (generic attribute) projection
/// </summary>
public record MetafieldInfo
{
    public string EntityType { get; set; }

    public int EntityId { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }
}