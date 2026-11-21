namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a light-weight manufacturer projection
/// </summary>
public record ManufacturerInfo
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool Published { get; set; }

    public string SeName { get; set; }
}