using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Api.Rest.Infrastructure;
using Nop.Plugin.Api.Rest.Mappings;
using Nop.Plugin.Api.Rest.Models;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;

namespace Nop.Plugin.Api.Rest.Controllers;

/// <summary>
/// Read-only storefront catalog endpoints for mobile and web clients
/// </summary>
[ApiController]
[Route("api/rest/store/catalog")]
public class CatalogController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IProductService _productService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IAclService _aclService;
    private readonly IProductModelFactory _productModelFactory;

    public CatalogController(
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IProductService productService,
        IStoreMappingService storeMappingService,
        IAclService aclService,
        IProductModelFactory productModelFactory)
    {
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _productService = productService;
        _storeMappingService = storeMappingService;
        _aclService = aclService;
        _productModelFactory = productModelFactory;
    }

    /// <summary>
    /// Returns categories configured
    /// </summary>
    [HttpGet("categories/home-page")]
    public async Task<ActionResult<List<CategoryDto>>> GetHomePageCategories()
    {
        var categories = await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync();
        var result = new List<CategoryDto>();

        foreach (var category in categories)
        {
            if (await IsVisibleAsync(category))
                result.Add(category.ToDto());
        }

        return Ok(result);
    }

    /// <summary>
    /// Returns root categories or the children of a specified category
    /// </summary>
    /// <param name="parentCategoryId">Parent category identifier; defaults to root categories</param>
    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories([FromQuery] int parentCategoryId = 0)
    {
        if (parentCategoryId < 0)
            return BadRequest(new { error = "The parent category identifier cannot be negative." });

        if (parentCategoryId > 0)
        {
            var parent = await _categoryService.GetCategoryByIdAsync(parentCategoryId);
            if (parent == null || !await IsVisibleAsync(parent))
                return NotFound();
        }

        var categories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(parentCategoryId);
        var result = new List<CategoryDto>();

        foreach (var category in categories)
        {
            if (await IsVisibleAsync(category))
                result.Add(category.ToDto());
        }

        return Ok(result);
    }

    /// <summary>
    /// Returns a published category
    /// </summary>
    /// <param name="id">Category identifier</param>
    [HttpGet("categories/{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null || !await IsVisibleAsync(category))
            return NotFound();

        return Ok(category.ToDto());
    }

    /// <summary>
    /// Returns products in a published category
    /// </summary>
    /// <param name="id">Category identifier</param>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Page size, capped at 200</param>
    [HttpGet("categories/{id:int}/products")]
    public async Task<ActionResult<PagedResult<StoreProductOverviewDto>>> GetCategoryProducts(int id,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = PagingHelper.DEFAULT_PAGE_SIZE)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null || !await IsVisibleAsync(category))
            return NotFound();

        var products = await _productService.SearchProductsAsync(
            pageIndex: PagingHelper.NormalizePageIndex(pageIndex),
            pageSize: PagingHelper.NormalizePageSize(pageSize),
            categoryIds: new List<int> { id },
            storeId: GetRequestedStoreId(),
            visibleIndividuallyOnly: true,
            showHidden: false,
            overridePublished: true);

        return Ok(await products.ToPagedStoreOverviewDtoAsync(_productModelFactory));
    }

    /// <summary>
    /// Returns published manufacturers
    /// </summary>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Page size, capped at 200</param>
    [HttpGet("manufacturers")]
    public async Task<ActionResult<PagedResult<ManufacturerDto>>> GetManufacturers(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = PagingHelper.DEFAULT_PAGE_SIZE)
    {
        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(
            storeId: GetRequestedStoreId(),
            pageIndex: PagingHelper.NormalizePageIndex(pageIndex),
            pageSize: PagingHelper.NormalizePageSize(pageSize),
            overridePublished: true);
        var result = manufacturers.ToPagedResult();
        result.Items = result.Items.Where(manufacturer => !manufacturer.Deleted).ToList();

        return Ok(result.Map(manufacturer => manufacturer.ToDto()));
    }

    /// <summary>
    /// Returns products from a published manufacturer
    /// </summary>
    /// <param name="id">Manufacturer identifier</param>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Page size, capped at 200</param>
    [HttpGet("manufacturers/{id:int}/products")]
    public async Task<ActionResult<PagedResult<StoreProductOverviewDto>>> GetManufacturerProducts(int id,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = PagingHelper.DEFAULT_PAGE_SIZE)
    {
        var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(id);
        if (manufacturer == null || !manufacturer.Published || manufacturer.Deleted
            || !await _storeMappingService.AuthorizeAsync(manufacturer, GetRequestedStoreId())
            || !await _aclService.AuthorizeAsync(manufacturer))
            return NotFound();

        var products = await _productService.SearchProductsAsync(
            pageIndex: PagingHelper.NormalizePageIndex(pageIndex),
            pageSize: PagingHelper.NormalizePageSize(pageSize),
            manufacturerIds: new List<int> { id },
            storeId: GetRequestedStoreId(),
            visibleIndividuallyOnly: true,
            showHidden: false,
            overridePublished: true);

        return Ok(await products.ToPagedStoreOverviewDtoAsync(_productModelFactory));
    }

    private async Task<bool> IsVisibleAsync(Nop.Core.Domain.Catalog.Category category)
        => category.Published && !category.Deleted
            && await _storeMappingService.AuthorizeAsync(category, GetRequestedStoreId())
            && await _aclService.AuthorizeAsync(category);

    private int GetRequestedStoreId()
    {
        var header = Request.Headers[ApiRestDefaults.StoreIdHeaderName].ToString();

        return int.TryParse(header, out var storeId) && storeId > 0 ? storeId : 0;
    }
}
