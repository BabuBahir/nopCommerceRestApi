namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a light-weight category projection
/// </summary>
public record CategoryInfo
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int ParentCategoryId { get; set; }

    public bool Published { get; set; }

    public string SeName { get; set; }
}