using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Rest.Infrastructure;
using Nop.Plugin.Api.Rest.Mappings;
using Nop.Plugin.Api.Rest.Models;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;

namespace Nop.Plugin.Api.Rest.Controllers;

/// <summary>
/// The catalog as a browsing client sees it
/// </summary>
/// <remarks>
/// A read only projection for storefront and mobile clients, kept apart from
/// <c>/api/rest/products</c> so that the admin view of a product and the shop view of it can evolve
/// separately. Part of the public store side of the API, so a customer token is accepted here and an
/// admin level credential is not. Follows the same read rule as every other operation: anonymous when
/// the read setting is off, behind a customer token when it is on.
/// </remarks>
[ApiController]
[Route("api/rest/store/products")]
public class ProductController : ControllerBase
{
    #region Fields

    private readonly IProductService _productService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IProductTagService _productTagService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IAclService _aclService;
    private readonly IProductModelFactory _productModelFactory;

    #endregion

    #region Ctor

    public ProductController(IProductService productService,
        IProductAttributeService productAttributeService,
        IManufacturerService manufacturerService,
        IProductTagService productTagService,
        IStoreMappingService storeMappingService,
        IAclService aclService,
        IProductModelFactory productModelFactory)
    {
        _productService = productService;
        _productAttributeService = productAttributeService;
        _manufacturerService = manufacturerService;
        _productTagService = productTagService;
        _storeMappingService = storeMappingService;
        _aclService = aclService;
        _productModelFactory = productModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the products featured on the store homepage
    /// </summary>
    [HttpGet("home-page")]
    public async Task<ActionResult<List<StoreProductOverviewDto>>> GetHomePageProducts()
    {
        var products = await _productService.GetAllProductsDisplayedOnHomepageAsync();
        var visibleProducts = new List<Product>();
        var storeId = GetRequestedStoreId();

        foreach (var product in products)
        {
            if (product.Published && !product.Deleted
                && await _storeMappingService.AuthorizeAsync(product, storeId)
                && await _aclService.AuthorizeAsync(product))
                visibleProducts.Add(product);
        }

        return Ok(await _productModelFactory.PrepareStoreOverviewDtosAsync(visibleProducts));
    }

    /// <summary>
    /// Returns products marked as new, newest first
    /// </summary>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Page size, capped at 200</param>
    [HttpGet("new")]
    public async Task<ActionResult<PagedResult<StoreProductOverviewDto>>> GetNewProducts(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = PagingHelper.DEFAULT_PAGE_SIZE)
    {
        var products = await _productService.GetProductsMarkedAsNewAsync(
            storeId: GetRequestedStoreId(),
            pageIndex: PagingHelper.NormalizePageIndex(pageIndex),
            pageSize: PagingHelper.NormalizePageSize(pageSize));

        return Ok(await products.ToPagedStoreOverviewDtoAsync(_productModelFactory));
    }

    /// <summary>
    /// Searches published products
    /// </summary>
    /// <param name="keywords">Optional words to search for</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="manufacturerId">Optional manufacturer filter</param>
    /// <param name="priceMin">Optional minimum price</param>
    /// <param name="priceMax">Optional maximum price</param>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Page size, capped at 200</param>
    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<StoreProductOverviewDto>>> Search(
        [FromQuery] string? keywords = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? manufacturerId = null,
        [FromQuery] decimal? priceMin = null,
        [FromQuery] decimal? priceMax = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = PagingHelper.DEFAULT_PAGE_SIZE)
    {
        if (priceMin < 0 || priceMax < 0 || priceMin > priceMax)
            return BadRequest(new { error = "The price range is invalid." });

        var products = await _productService.SearchProductsAsync(
            pageIndex: PagingHelper.NormalizePageIndex(pageIndex),
            pageSize: PagingHelper.NormalizePageSize(pageSize),
            categoryIds: categoryId is > 0 ? new List<int> { categoryId.Value } : null,
            manufacturerIds: manufacturerId is > 0 ? new List<int> { manufacturerId.Value } : null,
            storeId: GetRequestedStoreId(),
            visibleIndividuallyOnly: true,
            priceMin: priceMin,
            priceMax: priceMax,
            keywords: keywords,
            searchDescriptions: true,
            showHidden: false,
            overridePublished: true);

        return Ok(await products.ToPagedStoreOverviewDtoAsync(_productModelFactory));
    }

    /// <summary>
    /// Returns the published products related
    /// </summary>
    /// <param name="id">The product identifier</param>
    [HttpGet("{id:int}/related")]
    public async Task<ActionResult<List<StoreProductOverviewDto>>> GetRelatedProducts(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null || !product.Published || product.Deleted
            || !await _storeMappingService.AuthorizeAsync(product, GetRequestedStoreId())
            || !await _aclService.AuthorizeAsync(product))
            return NotFound();

        var related = await _productService.GetRelatedProductsByProductId1Async(id);
        var relatedIds = related.Select(item => item.ProductId2).Distinct().ToArray();
        var products = await _productService.GetProductsByIdsAsync(relatedIds);
        var productsById = products.ToDictionary(item => item.Id);
        var result = new List<Product>();

        foreach (var relatedProduct in related)
        {
            if (!productsById.TryGetValue(relatedProduct.ProductId2, out var item)
                || !item.Published || item.Deleted
                || !await _storeMappingService.AuthorizeAsync(item, GetRequestedStoreId())
                || !await _aclService.AuthorizeAsync(item))
                continue;

            result.Add(item);
        }

        return Ok(await _productModelFactory.PrepareStoreOverviewDtosAsync(result));
    }

    /// <summary>
    /// Returns a published product as a storefront client sees it
    /// </summary>
    /// <param name="id">The product identifier</param>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StoreProductDto>> GetById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        //an unpublished or deleted product is reported as missing rather than returned, because the
        //storefront must not be able to tell the difference
        if (product == null || !product.Published || product.Deleted)
            return NotFound();

        //published is not the same as listed: a product can be published yet mapped to other stores only,
        //so on a multi store install the header decides which catalog the caller is asking about. Omitting
        //it leaves the product unfiltered, which is how this endpoint behaved before the header existed.
        if (!await _storeMappingService.AuthorizeAsync(product, GetRequestedStoreId()))
            return NotFound();

        var combinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
        var manufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);
        var tags = await _productTagService.GetAllProductTagsByProductIdAsync(product.Id);

        return Ok(product.ToStoreDto(combinations, manufacturers, tags));
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Reads the store the caller says it is browsing, if it said
    /// </summary>
    /// <returns>The store identifier, or 0 when the header is absent or not a number</returns>
    /// <remarks>
    /// The header can only narrow what is returned, never widen it: a store of 0 means "no filtering" and
    /// any value the caller sends is still checked against the product's own store mapping. So an
    /// unparseable or absent value degrades to the previous behaviour rather than failing the request,
    /// which keeps clients that predate the header working.
    /// </remarks>
    protected virtual int GetRequestedStoreId()
    {
        var header = Request.Headers[ApiRestDefaults.StoreIdHeaderName].ToString();

        return int.TryParse(header, out var storeId) && storeId > 0 ? storeId : 0;
    }

    #endregion
}