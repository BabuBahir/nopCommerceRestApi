using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Rest.Models;
using Nop.Plugin.Api.Rest.Models.Requests;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Rest.Controllers;

/// <summary>
/// Shopping cart operations scoped to the customer token and current store
/// </summary>
[ApiController]
[Route("api/rest/customer/me/cart")]
public class ShoppingCartController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;
    private readonly IStoreContext _storeContext;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IAclService _aclService;

    public ShoppingCartController(ICustomerService customerService,
        IShoppingCartService shoppingCartService,
        IProductService productService,
        IStoreContext storeContext,
        IStoreMappingService storeMappingService,
        IAclService aclService)
    {
        _customerService = customerService;
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _storeContext = storeContext;
        _storeMappingService = storeMappingService;
        _aclService = aclService;
    }

    /// <summary>
    /// Gets the authenticated customer's cart for the current store
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CustomerCartDto>> GetCart()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null)
            return NotFound();

        return Ok(await BuildCartAsync(customer));
    }

    /// <summary>
    /// Adds a published product to the authenticated customer's cart
    /// </summary>
    /// <param name="request">The product, quantity, and selected product attributes</param>
    [HttpPost("items")]
    public async Task<ActionResult<CustomerCartDto>> AddItem([FromBody] AddCartItemRequest request)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null)
            return NotFound();

        var product = await _productService.GetProductByIdAsync(request.ProductId);
        var store = await _storeContext.GetCurrentStoreAsync();
        if (product == null || !product.Published || product.Deleted || !product.VisibleIndividually
            || !await _storeMappingService.AuthorizeAsync(product, store.Id)
            || !await _aclService.AuthorizeAsync(product))
            return NotFound();

        var warnings = await _shoppingCartService.AddToCartAsync(customer, product,
            ShoppingCartType.ShoppingCart, store.Id, request.AttributesXml, quantity: request.Quantity);
        if (warnings.Count > 0)
            return BadRequest(new { errors = warnings });

        return Ok(await BuildCartAsync(customer));
    }

    /// <summary>
    /// Changes the quantity of one item owned by the authenticated customer
    /// </summary>
    /// <param name="itemId">Shopping cart item identifier</param>
    /// <param name="request">The new quantity</param>
    [HttpPatch("items/{itemId:int}")]
    public async Task<ActionResult<CustomerCartDto>> UpdateItem(int itemId,
        [FromBody] UpdateCartItemRequest request)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null)
            return NotFound();

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer,
            ShoppingCartType.ShoppingCart, store.Id);
        var item = cart.FirstOrDefault(cartItem => cartItem.Id == itemId);
        if (item == null)
            return NotFound();

        var warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(customer, item.Id,
            item.AttributesXml, item.CustomerEnteredPrice, item.RentalStartDateUtc,
            item.RentalEndDateUtc, request.Quantity);
        if (warnings.Count > 0)
            return BadRequest(new { errors = warnings });

        return Ok(await BuildCartAsync(customer));
    }

    /// <summary>
    /// Removes one item owned by the authenticated customer
    /// </summary>
    /// <param name="itemId">Shopping cart item identifier</param>
    [HttpDelete("items/{itemId:int}")]
    public async Task<ActionResult<CustomerCartDto>> RemoveItem(int itemId)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null)
            return NotFound();

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer,
            ShoppingCartType.ShoppingCart, store.Id);
        var item = cart.FirstOrDefault(cartItem => cartItem.Id == itemId);
        if (item == null)
            return NotFound();

        await _shoppingCartService.DeleteShoppingCartItemAsync(item);

        return Ok(await BuildCartAsync(customer));
    }

    private async Task<CustomerCartDto> BuildCartAsync(Customer customer)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var items = await _shoppingCartService.GetShoppingCartAsync(customer,
            ShoppingCartType.ShoppingCart, store.Id);
        var products = items.Count == 0
            ? new List<Product>()
            : (await _productService.GetProductsByIdsAsync(items.Select(item => item.ProductId).Distinct().ToArray())).ToList();
        var productsById = products.ToDictionary(product => product.Id);
        var result = new CustomerCartDto
        {
            StoreId = store.Id,
            ItemCount = items.Count,
            TotalQuantity = items.Sum(item => item.Quantity)
        };

        foreach (var item in items)
        {
            productsById.TryGetValue(item.ProductId, out var product);
            var available = product != null && product.Published && !product.Deleted
                && await _storeMappingService.AuthorizeAsync(product, store.Id)
                && await _aclService.AuthorizeAsync(product);
            var unitPrice = 0m;

            if (available)
            {
                var price = await _shoppingCartService.GetUnitPriceAsync(item, includeDiscounts: true);
                unitPrice = price.unitPrice;
            }

            result.Items.Add(new CustomerCartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = product?.Name,
                Sku = product?.Sku,
                Quantity = item.Quantity,
                AttributesXml = item.AttributesXml,
                UnitPrice = unitPrice,
                ProductAvailable = available
            });
        }

        return result;
    }

    private async Task<Customer> GetCurrentCustomerAsync()
    {
        var claim = User?.FindFirst(ApiRestDefaults.CustomerIdClaim)?.Value;
        if (!int.TryParse(claim, out var customerId) || customerId <= 0)
            return null;

        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        return customer == null || customer.Deleted || !customer.Active ? null : customer;
    }
}
