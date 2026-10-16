using System.ComponentModel;
using ModelContextProtocol.Server;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;
using Nop.Plugin.Misc.Mcp.Dto;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Services.Shipping;

namespace Nop.Plugin.Misc.Mcp.MCP.Tools;

/// <summary>
/// Represents the order-related MCP tools
/// </summary>
[McpServerToolType]
public class OrderTools : BaseMcpTool
{
    #region Fields

    protected readonly IOrderService _orderService;
    protected readonly IOrderProcessingService _orderProcessingService;
    protected readonly IShipmentService _shipmentService;
    protected readonly IAddressService _addressService;
    protected readonly IProductService _productService;

    #endregion

    #region Ctor

    public OrderTools(IOrderService orderService,
        IOrderProcessingService orderProcessingService,
        IShipmentService shipmentService,
        IAddressService addressService,
        IProductService productService,
        McpSettings mcpSettings)
        : base(mcpSettings)
    {
        _orderService = orderService;
        _orderProcessingService = orderProcessingService;
        _shipmentService = shipmentService;
        _addressService = addressService;
        _productService = productService;
    }

    #endregion

    #region Tools

    /// <summary>
    /// Gets a single order by its identifier
    /// </summary>
    [McpServerTool(Name = "get_order_by_id")]
    [Description("Returns the full detail of an order (including its line items and billing address) by its identifier, or null when the order does not exist.")]
    public async Task<OrderInfo> GetOrderByIdAsync(
        [Description("The order identifier.")] int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order is null || order.Deleted)
            return null;

        return await PrepareOrderInfoAsync(order);
    }

    /// <summary>
    /// Searches for orders
    /// </summary>
    [McpServerTool(Name = "get_orders")]
    [Description("Returns a paginated list of orders, optionally filtered by customer, order/payment/shipping status and creation date range.")]
    public async Task<PagedResult<OrderInfo>> GetOrdersAsync(
        [Description("Only return orders placed by this customer identifier. 0 or null to skip the filter.")] int? customerId = null,
        [Description("Only return orders billed to this email address.")] string billingEmail = null,
        [Description("Only return orders billed to this phone number.")] string billingPhone = null,
        [Description("Only return orders in this order status (10 Pending, 20 Processing, 30 Complete, 40 Cancelled). Null to skip.")] int? orderStatusId = null,
        [Description("Only return orders in this payment status (10 Pending, 20 Authorized, 30 Paid, 40 Refunded, 50 Voided). Null to skip.")] int? paymentStatusId = null,
        [Description("Only return orders in this shipping status (10 Not required, 20 Not yet shipped, 25 Partially shipped, 30 Shipped, 40 Delivered). Null to skip.")] int? shippingStatusId = null,
        [Description("Only return orders created on or after this UTC timestamp.")] DateTime? createdFromUtc = null,
        [Description("Only return orders created on or before this UTC timestamp.")] DateTime? createdToUtc = null,
        [Description("Zero-based page index.")] int pageIndex = 0,
        [Description("Page size. Cannot exceed the configured maximum.")] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
        return await SearchOrdersCoreAsync(customerId, billingEmail, billingPhone, orderStatusId, paymentStatusId,
            shippingStatusId, createdFromUtc, createdToUtc, pageIndex, pageSize);
    }

    /// <summary>
    /// Finds orders by order number or billing contact information
    /// </summary>
    [McpServerTool(Name = "search_orders")]
    [Description("Finds orders by an exact custom order number (priority, returns at most one order) or falls back to searching by billing email/phone combined with statuses and creation date.")]
    public async Task<PagedResult<OrderInfo>> SearchOrdersAsync(
        [Description("Exact custom order number (for example \"1003\"). When provided this takes priority and returns at most one order.")] string orderNumber = null,
        [Description("Only return orders billed to this email address.")] string billingEmail = null,
        [Description("Only return orders billed to this phone number.")] string billingPhone = null,
        [Description("Only return orders in this order status (10 Pending, 20 Processing, 30 Complete, 40 Cancelled). Null to skip.")] int? orderStatusId = null,
        [Description("Only return orders in this payment status (10 Pending, 20 Authorized, 30 Paid, 40 Refunded, 50 Voided). Null to skip.")] int? paymentStatusId = null,
        [Description("Only return orders in this shipping status (10 Not required, 20 Not yet shipped, 25 Partially shipped, 30 Shipped, 40 Delivered). Null to skip.")] int? shippingStatusId = null,
        [Description("Only return orders created on or after this UTC timestamp.")] DateTime? createdFromUtc = null,
        [Description("Only return orders created on or before this UTC timestamp.")] DateTime? createdToUtc = null,
        [Description("Zero-based page index.")] int pageIndex = 0,
        [Description("Page size. Cannot exceed the configured maximum.")] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(orderNumber))
        {
            var order = await _orderService.GetOrderByCustomOrderNumberAsync(orderNumber.Trim());

            var result = new PagedResult<OrderInfo>();
            if (order is null || order.Deleted)
                return result;

            result.TotalCount = 1;
            result.TotalPages = 1;
            result.Items.Add(await PrepareOrderInfoAsync(order));
            return result;
        }

        return await SearchOrdersCoreAsync(customerId: null, billingEmail, billingPhone, orderStatusId, paymentStatusId,
            shippingStatusId, createdFromUtc, createdToUtc, pageIndex, pageSize);
    }

    /// <summary>
    /// Marks an order as paid
    /// </summary>
    [McpServerTool(Name = "mark_order_as_paid")]
    [Description("Marks an order as paid, which moves it to the 'processing' or 'complete' order state. Write tool; only available when write tools are enabled.")]
    public async Task<OrderInfo> MarkOrderAsPaidAsync(
        [Description("The order identifier.")] int orderId,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order is null || order.Deleted)
            return null;

        if (_orderProcessingService.CanMarkOrderAsPaid(order))
            await _orderProcessingService.MarkOrderAsPaidAsync(order);

        return await PrepareOrderInfoAsync(await _orderService.GetOrderByIdAsync(orderId));
    }

    /// <summary>
    /// Cancels an order
    /// </summary>
    [McpServerTool(Name = "cancel_order")]
    [Description("Cancels an order and (optionally) notifies the customer by email. Write tool; only available when write tools are enabled.")]
    public async Task<OrderInfo> CancelOrderAsync(
        [Description("The order identifier.")] int orderId,
        [Description("A value indicating whether to notify the customer by email.")] bool notifyCustomer = false,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order is null || order.Deleted)
            return null;

        if (_orderProcessingService.CanCancelOrder(order))
            await _orderProcessingService.CancelOrderAsync(order, notifyCustomer);

        return await PrepareOrderInfoAsync(await _orderService.GetOrderByIdAsync(orderId));
    }

    /// <summary>
    /// Creates a shipment for an order
    /// </summary>
    [McpServerTool(Name = "create_shipment")]
    [Description("Creates a shipment for an order by moving order items into a new shipment record. When no items are provided, every item that can still be shipped is added at full quantity. Write tool; only available when write tools are enabled.")]
    public async Task<ShipmentInfo> CreateShipmentAsync(
        [Description("The order identifier to ship.")] int orderId,
        [Description("Shipment items with explicit quantities. When empty, every remaining shippable item is included in full.")] IList<ShipmentItemInput> items = null,
        [Description("Carrier tracking number, if any.")] string trackingNumber = null,
        [Description("Admin comment.")] string adminComment = null,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order is null || order.Deleted)
            return null;

        var orderItems = await _orderService.GetOrderItemsAsync(orderId, isShipEnabled: true);

        //calculate how many units of each order item can still be added to shipments
        var maxQtyByOrderItemId = new Dictionary<int, int>();
        foreach (var orderItem in orderItems)
        {
            var maxQty = await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);
            if (maxQty > 0)
                maxQtyByOrderItemId[orderItem.Id] = maxQty;
        }

        if (maxQtyByOrderItemId.Count == 0)
            throw new InvalidOperationException("The order has no items that can be added to a shipment.");

        //resolve the quantities to ship
        var itemsToShip = new List<(OrderItem OrderItem, int Quantity)>();
        if (items is null || items.Count == 0)
        {
            foreach (var (orderItemId, maxQty) in maxQtyByOrderItemId)
                itemsToShip.Add((orderItems.First(oi => oi.Id == orderItemId), maxQty));
        }
        else
        {
            var seenOrderItemIds = new HashSet<int>();
            foreach (var requested in items)
            {
                if (!maxQtyByOrderItemId.TryGetValue(requested.OrderItemId, out var maxQty))
                    throw new InvalidOperationException($"Order item {requested.OrderItemId} does not belong to the order or has no quantity left to ship.");

                if (requested.Quantity <= 0)
                    throw new InvalidOperationException($"Quantity must be greater than zero for order item {requested.OrderItemId}.");

                if (requested.Quantity > maxQty)
                    throw new InvalidOperationException($"Quantity {requested.Quantity} exceeds the shippable quantity {maxQty} of order item {requested.OrderItemId}.");

                seenOrderItemIds.Add(requested.OrderItemId);
                itemsToShip.Add((orderItems.First(oi => oi.Id == requested.OrderItemId), requested.Quantity));
            }

            if (seenOrderItemIds.Count == 0)
                throw new InvalidOperationException("No valid shipment items were provided.");
        }

        var shipment = new Shipment
        {
            OrderId = orderId,
            TrackingNumber = trackingNumber,
            AdminComment = adminComment,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _shipmentService.InsertShipmentAsync(shipment);

        foreach (var (orderItem, quantity) in itemsToShip)
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            var shipmentItem = new ShipmentItem
            {
                ShipmentId = shipment.Id,
                OrderItemId = orderItem.Id,
                Quantity = quantity,
                WarehouseId = product?.WarehouseId ?? 0
            };

            await _shipmentService.InsertShipmentItemAsync(shipmentItem);
        }

        return await PrepareShipmentInfoAsync(shipment);
    }

    /// <summary>
    /// Marks a shipment as shipped
    /// </summary>
    [McpServerTool(Name = "mark_shipment_as_shipped")]
    [Description("Marks a shipment as shipped and (optionally) notifies the customer by email. Write tool; only available when write tools are enabled.")]
    public async Task<ShipmentInfo> MarkShipmentAsShippedAsync(
        [Description("The shipment identifier.")] int shipmentId,
        [Description("A value indicating whether to notify the customer by email.")] bool notifyCustomer = false,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);

        if (shipment is null)
            return null;

        await _orderProcessingService.ShipAsync(shipment, notifyCustomer);

        return await PrepareShipmentInfoAsync(await _shipmentService.GetShipmentByIdAsync(shipmentId));
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Searches orders by the common filters
    /// </summary>
    protected async Task<PagedResult<OrderInfo>> SearchOrdersCoreAsync(int? customerId, string billingEmail,
        string billingPhone, int? orderStatusId, int? paymentStatusId, int? shippingStatusId,
        DateTime? createdFromUtc, DateTime? createdToUtc, int pageIndex, int pageSize)
    {
        var size = NormalizePageSize(pageSize);
        var osIds = orderStatusId is > 0 ? new List<int> { orderStatusId.Value } : null;
        var psIds = paymentStatusId is > 0 ? new List<int> { paymentStatusId.Value } : null;
        var ssIds = shippingStatusId is > 0 ? new List<int> { shippingStatusId.Value } : null;

        var orders = await _orderService.SearchOrdersAsync(
            customerId: customerId ?? 0,
            createdFromUtc: createdFromUtc,
            createdToUtc: createdToUtc,
            osIds: osIds,
            psIds: psIds,
            ssIds: ssIds,
            billingPhone: string.IsNullOrWhiteSpace(billingPhone) ? null : billingPhone.Trim(),
            billingEmail: string.IsNullOrWhiteSpace(billingEmail) ? null : billingEmail.Trim(),
            pageIndex: pageIndex < 0 ? 0 : pageIndex,
            pageSize: size);

        var items = new List<OrderInfo>(orders.Count);
        foreach (var order in orders)
            items.Add(await PrepareOrderInfoAsync(order, includeItems: false));

        return new PagedResult<OrderInfo>
        {
            TotalCount = orders.TotalCount,
            PageIndex = orders.PageIndex,
            PageSize = orders.PageSize,
            TotalPages = orders.TotalPages,
            Items = items
        };
    }

    /// <summary>
    /// Prepares a light-weight order projection
    /// </summary>
    protected async Task<OrderInfo> PrepareOrderInfoAsync(Order order, bool includeItems = true)
    {
        var info = new OrderInfo
        {
            Id = order.Id,
            OrderNumber = order.CustomOrderNumber,
            OrderGuid = order.OrderGuid,
            CustomerId = order.CustomerId,
            StoreId = order.StoreId,
            OrderStatus = order.OrderStatus.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            ShippingStatus = order.ShippingStatus.ToString(),
            PaymentMethodSystemName = order.PaymentMethodSystemName,
            OrderSubtotalInclTax = order.OrderSubtotalInclTax,
            OrderShippingInclTax = order.OrderShippingInclTax,
            OrderTax = order.OrderTax,
            OrderDiscount = order.OrderDiscount,
            OrderTotal = order.OrderTotal,
            RefundedAmount = order.RefundedAmount,
            CustomerCurrencyCode = order.CustomerCurrencyCode,
            VatNumber = order.VatNumber,
            ShippingMethod = order.ShippingMethod,
            CreatedOnUtc = order.CreatedOnUtc,
            PaidDateUtc = order.PaidDateUtc
        };

        if (includeItems)
        {
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            foreach (var item in orderItems)
            {
                info.Items.Add(new OrderItemInfo
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPriceInclTax = item.UnitPriceInclTax,
                    UnitPriceExclTax = item.UnitPriceExclTax,
                    PriceInclTax = item.PriceInclTax,
                    PriceExclTax = item.PriceExclTax,
                    DiscountAmountInclTax = item.DiscountAmountInclTax,
                    AttributeDescription = item.AttributeDescription,
                    ItemWeight = item.ItemWeight,
                    RentalStartDateUtc = item.RentalStartDateUtc,
                    RentalEndDateUtc = item.RentalEndDateUtc
                });
            }
        }

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
        if (billingAddress != null)
        {
            info.BillingFirstName = billingAddress.FirstName;
            info.BillingLastName = billingAddress.LastName;
            info.BillingEmail = billingAddress.Email;
            info.BillingCity = billingAddress.City;
            info.BillingPhoneNumber = billingAddress.PhoneNumber;
        }

        return info;
    }

    /// <summary>
    /// Prepares a light-weight shipment projection
    /// </summary>
    protected async Task<ShipmentInfo> PrepareShipmentInfoAsync(Shipment shipment)
    {
        var info = ShipmentInfo.FromEntity(shipment);
        if (info is null)
            return null;

        var shipmentItems = await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id);
        foreach (var item in shipmentItems)
        {
            info.Items.Add(new ShipmentItemInfo
            {
                Id = item.Id,
                ShipmentId = item.ShipmentId,
                OrderItemId = item.OrderItemId,
                Quantity = item.Quantity,
                WarehouseId = item.WarehouseId
            });
        }

        return info;
    }

    #endregion
}