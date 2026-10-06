namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a product attribute mapping projection
/// </summary>
public record ProductAttributeInfo
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int ProductAttributeId { get; set; }

    public string AttributeName { get; set; }

    public string TextPrompt { get; set; }

    public bool IsRequired { get; set; }

    public string AttributeControlType { get; set; }

    public int DisplayOrder { get; set; }

    public IList<ProductAttributeValueInfo> Values { get; set; } = new List<ProductAttributeValueInfo>();
}