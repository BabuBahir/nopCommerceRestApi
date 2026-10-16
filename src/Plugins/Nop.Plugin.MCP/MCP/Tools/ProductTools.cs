using System.ComponentModel;
using ModelContextProtocol.Server;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;
using Nop.Plugin.Misc.Mcp.Dto;
using Nop.Services.Catalog;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.Mcp.MCP.Tools;

/// <summary>
/// Represents the product-related MCP tools
/// </summary>
[McpServerToolType]
public class ProductTools : BaseMcpTool
{
    #region Fields

    protected readonly IProductService _productService;
    protected readonly IProductAttributeService _productAttributeService;
    protected readonly IProductTagService _productTagService;
    protected readonly ICategoryService _categoryService;
    protected readonly IManufacturerService _manufacturerService;
    protected readonly IUrlRecordService _urlRecordService;

    #endregion

    #region Ctor

    public ProductTools(IProductService productService,
        IProductAttributeService productAttributeService,
        IProductTagService productTagService,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IUrlRecordService urlRecordService,
        McpSettings mcpSettings)
        : base(mcpSettings)
    {
        _productService = productService;
        _productAttributeService = productAttributeService;
        _productTagService = productTagService;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _urlRecordService = urlRecordService;
    }

    #endregion

    #region Tools

    /// <summary>
    /// Creates a product
    /// </summary>
    [McpServerTool(Name = "create_product")]
    [Description("Creates a new product with the given details. Write tool; only available when write tools are enabled.")]
    public async Task<ProductInfo> CreateProductAsync(
        [Description("The product name.")] string name,
        [Description("The product SKU.")] string sku = null,
        [Description("The product price in the primary store currency.")] decimal price = 0,
        [Description("The product old (compare-at) price in the primary store currency.")] decimal oldPrice = 0,
        [Description("The short description.")] string shortDescription = null,
        [Description("The full description.")] string fullDescription = null,
        [Description("The product type (5 Simple, 10 Grouped). Defaults to Simple.")] int productTypeId = 5,
        [Description("The initial stock quantity.")] int stockQuantity = 0,
        [Description("A value indicating whether the product is published. Defaults to true.")] bool published = true,
        [Description("A value indicating whether the product is visible individually. Defaults to true.")] bool visibleIndividually = true,
        [Description("A value indicating whether the product is shown on the home page.")] bool showOnHomepage = false,
        [Description("A value indicating whether customers can review the product. Defaults to true.")] bool allowCustomerReviews = true,
        [Description("The minimum order quantity. Defaults to 1.")] int orderMinimumQuantity = 1,
        [Description("The maximum order quantity. Defaults to 10000.")] int orderMaximumQuantity = 10000,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        var product = new Product
        {
            Name = name,
            Sku = sku,
            Price = price,
            OldPrice = oldPrice,
            ShortDescription = shortDescription,
            FullDescription = fullDescription,
            ProductTypeId = productTypeId,
            StockQuantity = stockQuantity,
            Published = published,
            VisibleIndividually = visibleIndividually,
            ShowOnHomepage = showOnHomepage,
            AllowCustomerReviews = allowCustomerReviews,
            OrderMinimumQuantity = orderMinimumQuantity,
            OrderMaximumQuantity = orderMaximumQuantity,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        await _productService.InsertProductAsync(product);

        return await PrepareProductInfoAsync(product);
    }

    /// <summary>
    /// Updates a product
    /// </summary>
    [McpServerTool(Name = "update_product")]
    [Description("Updates the fields of an existing product. Parameters that are omitted or left at their default value are left unchanged. Write tool; only available when write tools are enabled.")]
    public async Task<ProductInfo> UpdateProductAsync(
        [Description("The product identifier to update.")] int productId,
        [Description("The product name.")] string name = null,
        [Description("The product SKU.")] string sku = null,
        [Description("The product price in the primary store currency. Use a null value to leave unchanged.")] decimal? price = null,
        [Description("The product old (compare-at) price in the primary store currency. Use a null value to leave unchanged.")] decimal? oldPrice = null,
        [Description("The short description. Pass an empty string to clear it.")] string shortDescription = null,
        [Description("The full description. Pass an empty string to clear it.")] string fullDescription = null,
        [Description("The product type (5 Simple, 10 Grouped). Use a null value to leave unchanged.")] int? productTypeId = null,
        [Description("A value indicating whether the product is published. Null leaves it unchanged.")] bool? published = null,
        [Description("A value indicating whether the product is visible individually. Null leaves it unchanged.")] bool? visibleIndividually = null,
        [Description("A value indicating whether the product is shown on the home page. Null leaves it unchanged.")] bool? showOnHomepage = null,
        [Description("A value indicating whether customers can review the product. Null leaves it unchanged.")] bool? allowCustomerReviews = null,
        [Description("The minimum order quantity. Null leaves it unchanged.")] int? orderMinimumQuantity = null,
        [Description("The maximum order quantity. Null leaves it unchanged.")] int? orderMaximumQuantity = null,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        var product = await _productService.GetProductByIdAsync(productId);

        if (product is null || product.Deleted)
            return null;

        if (!string.IsNullOrWhiteSpace(name))
            product.Name = name;
        if (sku is not null)
            product.Sku = sku;
        if (price is not null)
            product.Price = price.Value;
        if (oldPrice is not null)
            product.OldPrice = oldPrice.Value;
        if (shortDescription is not null)
            product.ShortDescription = shortDescription;
        if (fullDescription is not null)
            product.FullDescription = fullDescription;
        if (productTypeId is not null)
            product.ProductTypeId = productTypeId.Value;
        if (published is not null)
            product.Published = published.Value;
        if (visibleIndividually is not null)
            product.VisibleIndividually = visibleIndividually.Value;
        if (showOnHomepage is not null)
            product.ShowOnHomepage = showOnHomepage.Value;
        if (allowCustomerReviews is not null)
            product.AllowCustomerReviews = allowCustomerReviews.Value;
        if (orderMinimumQuantity is not null)
            product.OrderMinimumQuantity = orderMinimumQuantity.Value;
        if (orderMaximumQuantity is not null)
            product.OrderMaximumQuantity = orderMaximumQuantity.Value;

        product.UpdatedOnUtc = DateTime.UtcNow;

        await _productService.UpdateProductAsync(product);

        return await PrepareProductInfoAsync(product);
    }

    /// <summary>
    /// Gets the attribute mappings and values of a product
    /// </summary>
    [McpServerTool(Name = "get_product_attributes")]
    [Description("Returns all attribute mappings of a product together with their possible values.")]
    public async Task<IList<ProductAttributeInfo>> GetProductAttributesAsync(
        [Description("The product identifier.")] int productId,
        CancellationToken cancellationToken = default)
    {
        var mappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);

        var result = new List<ProductAttributeInfo>(mappings.Count);
        foreach (var mapping in mappings)
        {
            var attribute = await _productAttributeService.GetProductAttributeByIdAsync(mapping.ProductAttributeId);
            var values = await _productAttributeService.GetProductAttributeValuesAsync(mapping.Id);

            result.Add(new ProductAttributeInfo
            {
                Id = mapping.Id,
                ProductId = mapping.ProductId,
                ProductAttributeId = mapping.ProductAttributeId,
                AttributeName = attribute?.Name,
                TextPrompt = mapping.TextPrompt,
                IsRequired = mapping.IsRequired,
                AttributeControlType = mapping.AttributeControlType.ToString(),
                DisplayOrder = mapping.DisplayOrder,
                Values = values.Select(value => new ProductAttributeValueInfo
                {
                    Id = value.Id,
                    Name = value.Name,
                    PriceAdjustment = value.PriceAdjustment,
                    PriceAdjustmentUsePercentage = value.PriceAdjustmentUsePercentage,
                    WeightAdjustment = value.WeightAdjustment,
                    Cost = value.Cost,
                    IsPreSelected = value.IsPreSelected,
                    DisplayOrder = value.DisplayOrder
                }).ToList()
            });
        }

        return result;
    }

    /// <summary>
    /// Creates a product attribute combination
    /// </summary>
    [McpServerTool(Name = "create_product_attribute_combination")]
    [Description("Creates a new product attribute combination (stock-keeping unit defined by a specific set of attribute values). AttributesXml must be in the nopCommerce format, e.g. <Attributes><ProductAttribute ID=\"mappingId\"><ProductAttributeValue><Value>valueId</Value></ProductAttributeValue></ProductAttribute></Attributes>. Write tool; only available when write tools are enabled.")]
    public async Task<ProductCombinationInfo> CreateProductAttributeCombinationAsync(
        [Description("The product identifier the combination belongs to.")] int productId,
        [Description("The selected attribute values in nopCommerce AttributesXml format.")] string attributesXml,
        [Description("The stock quantity of the combination. Defaults to 0.")] int stockQuantity = 0,
        [Description("A value indicating whether to allow orders when out of stock. Defaults to false.")] bool allowOutOfStockOrders = false,
        [Description("The combination SKU.")] string sku = null,
        [Description("The combination manufacturer part number.")] string manufacturerPartNumber = null,
        [Description("The combination GTIN.")] string gtin = null,
        [Description("The overridden price of the combination. Null keeps the product price.")] decimal? overriddenPrice = null,
        [Description("The quantity below which the admin should be notified. Defaults to 0.")] int notifyAdminForQuantityBelow = 0,
        [Description("The minimum stock quantity of the combination. Defaults to 0.")] int minStockQuantity = 0,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        var product = await _productService.GetProductByIdAsync(productId);

        if (product is null || product.Deleted)
            return null;

        if (string.IsNullOrWhiteSpace(attributesXml))
            throw new InvalidOperationException("The attributesXml parameter is required and must describe the attribute values of the combination.");

        var combination = new ProductAttributeCombination
        {
            ProductId = productId,
            AttributesXml = attributesXml,
            StockQuantity = stockQuantity,
            AllowOutOfStockOrders = allowOutOfStockOrders,
            Sku = sku,
            ManufacturerPartNumber = manufacturerPartNumber,
            Gtin = gtin,
            OverriddenPrice = overriddenPrice,
            NotifyAdminForQuantityBelow = notifyAdminForQuantityBelow,
            MinStockQuantity = minStockQuantity
        };

        await _productAttributeService.InsertProductAttributeCombinationAsync(combination);

        return new ProductCombinationInfo
        {
            Id = combination.Id,
            ProductId = combination.ProductId,
            AttributesXml = combination.AttributesXml,
            StockQuantity = combination.StockQuantity,
            AllowOutOfStockOrders = combination.AllowOutOfStockOrders,
            Sku = combination.Sku,
            ManufacturerPartNumber = combination.ManufacturerPartNumber,
            Gtin = combination.Gtin,
            OverriddenPrice = combination.OverriddenPrice,
            NotifyAdminForQuantityBelow = combination.NotifyAdminForQuantityBelow,
            MinStockQuantity = combination.MinStockQuantity
        };
    }

    /// <summary>
    /// Sets the stock quantity of a product
    /// </summary>
    [McpServerTool(Name = "set_inventory")]
    [Description("Sets the stock quantity of a product (or of one of its attribute combinations) and persists the change. Write tool; only available when write tools are enabled.")]
    public async Task<ProductInfo> SetInventoryAsync(
        [Description("The product identifier.")] int productId,
        [Description("The new stock quantity.")] int stockQuantity,
        [Description("When provided, the stock quantity of this attribute combination is updated instead of the base product.")] int? combinationId = null,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        if (combinationId is > 0)
        {
            var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(combinationId.Value);
            if (combination is null || combination.ProductId != productId)
                throw new InvalidOperationException($"Combination {combinationId} was not found or does not belong to product {productId}.");

            combination.StockQuantity = stockQuantity;
            await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);

            return await PrepareProductInfoAsync(await _productService.GetProductByIdAsync(productId));
        }

        var product = await _productService.GetProductByIdAsync(productId);

        if (product is null || product.Deleted)
            return null;

        product.StockQuantity = stockQuantity;
        product.UpdatedOnUtc = DateTime.UtcNow;
        await _productService.UpdateProductAsync(product);

        return await PrepareProductInfoAsync(product);
    }

    /// <summary>
    /// Manages the tags of a product
    /// </summary>
    [McpServerTool(Name = "manage_tags")]
    [Description("Replaces the tag set of a product with the given tag names. Pass an empty array to remove all tags. Write tool; only available when write tools are enabled.")]
    public async Task<ProductInfo> ManageTagsAsync(
        [Description("The product identifier.")] int productId,
        [Description("The full list of tag names the product should be tagged with.")] string[] tags = null,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        var product = await _productService.GetProductByIdAsync(productId);

        if (product is null || product.Deleted)
            return null;

        var tagNames = (tags ?? Array.Empty<string>())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await _productTagService.UpdateProductTagsAsync(product, tagNames);

        return await PrepareProductInfoAsync(product);
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Prepares a light-weight product projection
    /// </summary>
    protected async Task<ProductInfo> PrepareProductInfoAsync(Product product)
    {
        var info = new ProductInfo
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            ShortDescription = product.ShortDescription,
            Price = product.Price,
            OldPrice = product.OldPrice,
            StockQuantity = product.StockQuantity,
            ProductType = ((ProductType)product.ProductTypeId).ToString(),
            Published = product.Published,
            CreatedOnUtc = product.CreatedOnUtc,
            UpdatedOnUtc = product.UpdatedOnUtc,
            SeName = await _urlRecordService.GetSeNameAsync(product)
        };

        var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, showHidden: true);
        if (productCategories.Any())
        {
            var categories = await _categoryService.GetCategoriesByIdsAsync(productCategories.Select(pc => pc.CategoryId).ToArray());
            info.Categories = categories.Select(c => c.Name).Distinct().ToList();
        }

        var productManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, showHidden: true);
        if (productManufacturers.Any())
        {
            var manufacturers = await _manufacturerService.GetManufacturersByIdsAsync(productManufacturers.Select(pm => pm.ManufacturerId).ToArray());
            info.Manufacturers = manufacturers.Select(m => m.Name).Distinct().ToList();
        }

        return info;
    }

    #endregion
}