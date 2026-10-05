using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.RazorpayPayment.Models;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;
using Razorpay.Api;
using Nop.Services.Common;
using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Payments.RazorpayPayment.Controllers;

public class PaymentRazorpayController : BasePaymentController
{
    private readonly IOrderService _orderService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly ILogger _logger;
    private readonly IWorkContext _workContext;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IAddressService _addressService;

    public PaymentRazorpayController(IOrderService orderService,
        IOrderProcessingService orderProcessingService,
        IStoreContext storeContext,
        ISettingService settingService,
        ILogger logger,
        IWorkContext workContext,
        IGenericAttributeService genericAttributeService,
        IAddressService addressService)
    {
        _orderService = orderService;
        _orderProcessingService = orderProcessingService;
        _storeContext = storeContext;
        _settingService = settingService;
        _logger = logger;
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
        _addressService = addressService;
    }

    public async Task<IActionResult> Checkout()
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var orderId = await _genericAttributeService.GetAttributeAsync<int>(customer, "RazorpayPaymentOrderId");
            if (orderId == 0)
                return RedirectToRoute("Homepage");

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.PaymentStatus == Nop.Core.Domain.Payments.PaymentStatus.Paid)
                return RedirectToRoute("Homepage");

            var settings = await _settingService.LoadSettingAsync<RazorpayPaymentSettings>(store.Id);

            // Razorpay takes amount in smallest currency unit (e.g. paise)
            decimal amountInPaise = Math.Round(order.OrderTotal * 100, 0);

            // Create Order on Razorpay
            RazorpayClient client = new RazorpayClient(settings.KeyId, settings.KeySecret);
            Dictionary<string, object> options = new Dictionary<string, object>
            {
                { "amount", amountInPaise },
                { "currency", "INR" }, // Usually INR, could be dynamic
                { "receipt", order.CustomOrderNumber }
            };

            Razorpay.Api.Order rzpOrder = client.Order.Create(options);
            string rzpOrderId = rzpOrder["id"].ToString();

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var model = new CheckoutModel
            {
                KeyId = settings.KeyId,
                RazorpayOrderId = rzpOrderId,
                Amount = amountInPaise,
                Currency = "INR",
                Description = $"Order #{order.CustomOrderNumber}",
                Name = store.Name,
                Email = billingAddress?.Email ?? customer.Email,
                Contact = billingAddress?.PhoneNumber ?? "",
                CallbackUrl = Url.RouteUrl("Plugin.Payments.RazorpayPayment.SuccessCallback", null, Request.Scheme)
            };

            // Save Razorpay order ID in custom values for verification later
            order.CustomValuesXml = rzpOrderId;
            await _orderService.UpdateOrderAsync(order);

            return View(model);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync(ex.Message, ex);
            return RedirectToRoute("Homepage");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SuccessCallback(IFormCollection form)
    {
        try
        {
            var razorpayPaymentId = form["razorpay_payment_id"].ToString();
            var razorpayOrderId = form["razorpay_order_id"].ToString();
            var razorpaySignature = form["razorpay_signature"].ToString();

            var store = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<RazorpayPaymentSettings>(store.Id);

            // Verify signature
            Dictionary<string, string> attributes = new Dictionary<string, string>
            {
                { "razorpay_payment_id", razorpayPaymentId },
                { "razorpay_order_id", razorpayOrderId },
                { "razorpay_signature", razorpaySignature }
            };

            Utils.verifyPaymentSignature(attributes); // Throws exception if invalid

            // Find NopCommerce order by matching CustomValuesXml
            var customer = await _workContext.GetCurrentCustomerAsync();
            var orderId = await _genericAttributeService.GetAttributeAsync<int>(customer, "RazorpayPaymentOrderId");
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order != null && order.CustomValuesXml == razorpayOrderId)
            {
                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);
                }

                // Clear the generic attribute
                await _genericAttributeService.SaveAttributeAsync<int>(customer, "RazorpayPaymentOrderId", 0);

                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }

            return RedirectToRoute("Homepage");
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Razorpay signature verification failed: " + ex.Message, ex);
            return RedirectToRoute("Homepage");
        }
    }
}