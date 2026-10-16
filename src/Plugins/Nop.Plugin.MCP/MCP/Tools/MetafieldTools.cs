using System.ComponentModel;
using ModelContextProtocol.Server;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;
using Nop.Plugin.Misc.Mcp.Dto;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.Mcp.MCP.Tools;

/// <summary>
/// Represents the metafield (generic attribute) related MCP tools
/// </summary>
[McpServerToolType]
public class MetafieldTools : BaseMcpTool
{
    #region Fields

    protected readonly IGenericAttributeService _genericAttributeService;

    protected static readonly Dictionary<string, Type> EntityTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["product"] = typeof(Product),
        ["category"] = typeof(Category),
        ["manufacturer"] = typeof(Manufacturer),
        ["customer"] = typeof(Customer),
        ["order"] = typeof(Order),
        ["orderitem"] = typeof(OrderItem),
        ["address"] = typeof(Address),
        ["vendor"] = typeof(Vendor),
        ["producttag"] = typeof(ProductTag),
        ["shipment"] = typeof(Shipment),
        ["discount"] = typeof(Discount),
        ["store"] = typeof(Store),
        ["country"] = typeof(Country),
        ["stateprovince"] = typeof(StateProvince),
        ["campaign"] = typeof(Campaign)
    };

    #endregion

    #region Ctor

    public MetafieldTools(IGenericAttributeService genericAttributeService,
        McpSettings mcpSettings)
        : base(mcpSettings)
    {
        _genericAttributeService = genericAttributeService;
    }

    #endregion

    #region Tools

    /// <summary>
    /// Reads a metafield
    /// </summary>
    [McpServerTool(Name = "get_metafield")]
    [Description("Returns a single metafield (nopCommerce generic attribute) value stored on an entity, or an empty value when not present.")]
    public async Task<MetafieldInfo> GetMetafieldAsync(
        [Description("Entity type the attribute is stored on, e.g. \"product\", \"customer\", \"category\", \"manufacturer\" or \"order\".")] string entityType,
        [Description("Entity identifier the attribute is stored on.")] int entityId,
        [Description("Metafield key.")] string key,
        CancellationToken cancellationToken = default)
    {
        if (!TryCreateEntity(entityType, entityId, out var entity))
            throw new InvalidOperationException($"Unsupported entity type '{entityType}'.");

        var value = await _genericAttributeService.GetAttributeAsync<string>(entity, key, defaultValue: null);

        return new MetafieldInfo
        {
            EntityType = entity.GetType().Name,
            EntityId = entityId,
            Key = key,
            Value = value
        };
    }

    /// <summary>
    /// Sets a metafield
    /// </summary>
    [McpServerTool(Name = "set_metafield")]
    [Description("Creates or updates a metafield (nopCommerce generic attribute) on an entity. Passing an empty or null value deletes the metafield if it exists. Write tool; only available when write tools are enabled.")]
    public async Task<MetafieldInfo> SetMetafieldAsync(
        [Description("Entity type the attribute is stored on, e.g. \"product\", \"customer\", \"category\", \"manufacturer\" or \"order\".")] string entityType,
        [Description("Entity identifier the attribute is stored on.")] int entityId,
        [Description("Metafield key.")] string key,
        [Description("Metafield value. Pass an empty string to delete the metafield.")] string value = null,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        if (!TryCreateEntity(entityType, entityId, out var entity))
            throw new InvalidOperationException($"Unsupported entity type '{entityType}'.");

        await _genericAttributeService.SaveAttributeAsync<string>(entity, key, value ?? string.Empty);
        var saved = await _genericAttributeService.GetAttributeAsync<string>(entity, key, defaultValue: null);

        return new MetafieldInfo
        {
            EntityType = entity.GetType().Name,
            EntityId = entityId,
            Key = key,
            Value = saved
        };
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Creates a shell entity instance of the given type with the identifier set. Used only to look up the key group and store the attribute.
    /// </summary>
    protected bool TryCreateEntity(string entityType, int entityId, out BaseEntity entity)
    {
        entity = null;

        if (string.IsNullOrWhiteSpace(entityType) || !EntityTypeMap.TryGetValue(entityType.Trim(), out var type))
            return false;

        entity = (BaseEntity)Activator.CreateInstance(type);
        entity.Id = entityId;

        return true;
    }

    #endregion
}