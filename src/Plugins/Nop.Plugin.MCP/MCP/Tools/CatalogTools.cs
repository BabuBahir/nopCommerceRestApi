using System.ComponentModel;
using ModelContextProtocol.Server;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;
using Nop.Plugin.Misc.Mcp.Dto;
using Nop.Services.Catalog;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.Mcp.MCP.Tools;

/// <summary>
/// Represents the catalog-related MCP tools
/// </summary>
[McpServerToolType]
public class CatalogTools
{
    #region Fields

    protected readonly ICategoryService _categoryService;
    protected readonly IManufacturerService _manufacturerService;
    protected readonly IProductService _productService;
    protected readonly IUrlRecordService _urlRecordService;
    protected readonly IStoreContext _storeContext;
    protected readonly McpSettings _mcpSettings;

    #endregion

    #region Ctor

    public CatalogTools(ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IProductService productService,
        IUrlRecordService urlRecordService,
        IStoreContext storeContext,
        McpSettings mcpSettings)
    {
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _productService = productService;
        _urlRecordService = urlRecordService;
        _storeContext = storeContext;
        _mcpSettings = mcpSettings;
    }

    #endregion

    #region Tools

    /// <summary>
    /// Searches for products in the store catalog
    /// </summary>
    [McpServerTool(Name = "get_products")]
    [Description("Returns a paginated list of published products matching the given filters (keyword, category, manufacturer, price range). Only visible, published products are returned.")]
    public async Task<PagedResult<ProductInfo>> GetProductsAsync(
        [Description("Search keyword matched against product name. Empty to list all products.")] string keywords = null,
        [Description("Only return products from this category identifier. 0 or null to skip the filter.")] int? categoryId = null,
        [Description("Only return products from this manufacturer identifier. 0 or null to skip the filter.")] int? manufacturerId = null,
        [Description("Minimum price filter (in the primary store currency). Null to skip.")] decimal? priceMin = null,
        [Description("Maximum price filter (in the primary store currency). Null to skip.")] decimal? priceMax = null,
        [Description("A value indicating whether to search by the keyword in the product SKU.")] bool searchSku = true,
        [Description("A value indicating whether to search by the keyword in the product descriptions.")] bool searchDescriptions = false,
        [Description("Zero-based page index.")] int pageIndex = 0,
        [Description("Page size. Cannot exceed the configured maximum.")] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        var size = PagingHelper.NormalizePageSize(_mcpSettings, pageSize);

        var products = await _productService.SearchProductsAsync(
            pageIndex: pageIndex < 0 ? 0 : pageIndex,
            pageSize: size,
            categoryIds: categoryId is > 0 ? new[] { categoryId.Value } : null,
            manufacturerIds: manufacturerId is > 0 ? new[] { manufacturerId.Value } : null,
            storeId: storeId,
            visibleIndividuallyOnly: true,
            priceMin: priceMin,
            priceMax: priceMax,
            keywords: string.IsNullOrWhiteSpace(keywords) ? null : keywords.Trim(),
            searchDescriptions: searchDescriptions,
            searchSku: searchSku,
            orderBy: ProductSortingEnum.Position,
            showHidden: false,
            overridePublished: true);

        var items = new List<ProductInfo>(products.Count);
        foreach (var product in products)
            items.Add(await PrepareProductInfoAsync(product));

        return new PagedResult<ProductInfo>
        {
            TotalCount = products.TotalCount,
            PageIndex = products.PageIndex,
            PageSize = products.PageSize,
            TotalPages = products.TotalPages,
            Items = items
        };
    }

    /// <summary>
    /// Gets a single product by its identifier
    /// </summary>
    [McpServerTool(Name = "get_product")]
    [Description("Returns details of a single published product by its identifier, or null when not found.")]
    public async Task<ProductInfo> GetProductAsync(
        [Description("The product identifier.")] int productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductByIdAsync(productId);

        if (product is null || product.Deleted || !product.Published || !product.VisibleIndividually)
            return null;

        return await PrepareProductInfoAsync(product);
    }

    /// <summary>
    /// Lists store categories
    /// </summary>
    [McpServerTool(Name = "get_categories")]
    [Description("Returns a paginated list of published categories, optionally filtered by name.")]
    public async Task<PagedResult<CategoryInfo>> GetCategoriesAsync(
        [Description("Optional search keyword matched against the category name.")] string name = null,
        [Description("Zero-based page index.")] int pageIndex = 0,
        [Description("Page size. Cannot exceed the configured maximum.")] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        var size = PagingHelper.NormalizePageSize(_mcpSettings, pageSize);

        var categories = await _categoryService.GetAllCategoriesAsync(
            categoryName: name ?? string.Empty,
            storeId: storeId,
            pageIndex: pageIndex < 0 ? 0 : pageIndex,
            pageSize: size,
            showHidden: false,
            overridePublished: true);

        var items = new List<CategoryInfo>(categories.Count);
        foreach (var category in categories)
        {
            items.Add(new CategoryInfo
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                Published = category.Published,
                SeName = await _urlRecordService.GetSeNameAsync(category)
            });
        }

        return new PagedResult<CategoryInfo>
        {
            TotalCount = categories.TotalCount,
            PageIndex = categories.PageIndex,
            PageSize = categories.PageSize,
            TotalPages = categories.TotalPages,
            Items = items
        };
    }

    /// <summary>
    /// Lists store manufacturers
    /// </summary>
    [McpServerTool(Name = "get_manufacturers")]
    [Description("Returns a paginated list of published manufacturers, optionally filtered by name.")]
    public async Task<PagedResult<ManufacturerInfo>> GetManufacturersAsync(
        [Description("Optional search keyword matched against the manufacturer name.")] string name = null,
        [Description("Zero-based page index.")] int pageIndex = 0,
        [Description("Page size. Cannot exceed the configured maximum.")] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        var size = PagingHelper.NormalizePageSize(_mcpSettings, pageSize);

        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(
            manufacturerName: name ?? string.Empty,
            storeId: storeId,
            pageIndex: pageIndex < 0 ? 0 : pageIndex,
            pageSize: size,
            showHidden: false,
            overridePublished: true);

        var items = new List<ManufacturerInfo>(manufacturers.Count);
        foreach (var manufacturer in manufacturers)
        {
            items.Add(new ManufacturerInfo
            {
                Id = manufacturer.Id,
                Name = manufacturer.Name,
                Description = manufacturer.Description,
                Published = manufacturer.Published,
                SeName = await _urlRecordService.GetSeNameAsync(manufacturer)
            });
        }

        return new PagedResult<ManufacturerInfo>
        {
            TotalCount = manufacturers.TotalCount,
            PageIndex = manufacturers.PageIndex,
            PageSize = manufacturers.PageSize,
            TotalPages = manufacturers.TotalPages,
            Items = items
        };
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

        var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, showHidden: false);
        if (productCategories.Any())
        {
            var categories = await _categoryService.GetCategoriesByIdsAsync(productCategories.Select(pc => pc.CategoryId).ToArray());
            info.Categories = categories.Select(c => c.Name).Distinct().ToList();
        }

        var productManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, showHidden: false);
        if (productManufacturers.Any())
        {
            var manufacturers = await _manufacturerService.GetManufacturersByIdsAsync(productManufacturers.Select(pm => pm.ManufacturerId).ToArray());
            info.Manufacturers = manufacturers.Select(m => m.Name).Distinct().ToList();
        }

        return info;
    }

    #endregion
}