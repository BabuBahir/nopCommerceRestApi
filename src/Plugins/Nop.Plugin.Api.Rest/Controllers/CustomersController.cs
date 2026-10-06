using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Rest.Infrastructure;
using Nop.Plugin.Api.Rest.Mappings;
using Nop.Plugin.Api.Rest.Models;
using Nop.Plugin.Api.Rest.Models.Requests;
using Nop.Services.Customers;

namespace Nop.Plugin.Api.Rest.Controllers
{
    [ApiController]
    [Route("api/rest/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IStoreContext _storeContext;
        private readonly CustomerSettings _customerSettings;

        public CustomersController(ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService,
            IStoreContext storeContext,
            CustomerSettings customerSettings)
        {
            _customerService = customerService;
            _customerRegistrationService = customerRegistrationService;
            _storeContext = storeContext;
            _customerSettings = customerSettings;
        }

        // GET api/rest/customers/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerDto>> GetById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer.ToDto());
        }

        // GET api/rest/customers?pageIndex=0&pageSize=20&email=&firstName=&customerRoleId=&isActive=
        [HttpGet]
        public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = PagingHelper.DEFAULT_PAGE_SIZE,
            [FromQuery] string? email = null,
            [FromQuery] string? firstName = null,
            [FromQuery] string? lastName = null,
            [FromQuery] string? phone = null,
            [FromQuery] int? customerRoleId = null,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            [FromQuery] bool? isActive = null)
        {
            var index = PagingHelper.NormalizePageIndex(pageIndex);
            var size = PagingHelper.NormalizePageSize(pageSize);

            var customers = await _customerService.GetAllCustomersAsync(
                createdFromUtc: createdFromUtc,
                createdToUtc: createdToUtc,
                //zero means "no filter", matching the convention the rest of the query string uses
                customerRoleIds: customerRoleId is > 0 ? new[] { customerRoleId.Value } : null,
                email: email,
                firstName: firstName,
                lastName: lastName,
                phone: phone,
                isActive: isActive,
                pageIndex: index,
                pageSize: size);

            return Ok(customers.ToPagedResult().Map(c => c.ToDto()));
        }

        // GET api/rest/customers/{id}/addresses
        [HttpGet("{id:int}/addresses")]
        public async Task<ActionResult<List<AddressDto>>> GetAddresses(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();

            var addresses = await _customerService.GetAddressesByCustomerIdAsync(id);

            return Ok(addresses.Select(a => a.ToDto()).ToList());
        }

        // POST api/rest/customers
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateCustomerRequest request)
        {
            if (request == null) return BadRequest(new { error = "A request body is required." });

            var email = request.Email?.Trim();
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email is required." });

            if (!CommonHelper.IsValidEmail(email))
                return BadRequest(new { error = $"'{email}' is not a valid email address." });

            if (string.IsNullOrEmpty(request.Password))
                return BadRequest(new { error = "Password is required." });

            var username = string.IsNullOrWhiteSpace(request.Username) ? email : request.Username.Trim();

            if (_customerSettings.UsernamesEnabled && string.IsNullOrEmpty(username))
                return BadRequest(new { error = "Username is required on a store that has usernames enabled." });

            //uniqueness is checked here as well as inside the registration service, so the record is
            //never inserted when it is going to be refused
            if (await _customerService.GetCustomerByEmailAsync(email) != null)
                return BadRequest(new { error = $"'{email}' is already registered." });

            if (_customerSettings.UsernamesEnabled && await _customerService.GetCustomerByUsernameAsync(username) != null)
                return BadRequest(new { error = $"Username '{username}' is already taken." });

            var active = request.Active ?? true;
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            var customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                FirstName = request.FirstName?.Trim() ?? string.Empty,
                LastName = request.LastName?.Trim() ?? string.Empty,
                Phone = request.Phone?.Trim() ?? string.Empty,
                Active = active,
                Deleted = false,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = storeId
            };

            await _customerService.InsertCustomerAsync(customer);

            //adds the Registered role, drops the Guests role when the caller passes a guest, stores
            //the password and sets Active from IsApproved. Every failure mode was pre-checked above,
            //so a failure here means the service changed under us rather than a caller mistake
            var registration = await _customerRegistrationService.RegisterCustomerAsync(
                new CustomerRegistrationRequest(customer, email, username, request.Password,
                    _customerSettings.DefaultPasswordFormat, storeId, isApproved: active));

            if (!registration.Success)
            {
                await _customerService.DeleteCustomerAsync(customer);
                return BadRequest(new { error = string.Join(" ", registration.Errors) });
            }

            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer.ToDto());
        }

        // PATCH api/rest/customers/{id}
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] UpdateCustomerRequest request)
        {
            if (request == null) return BadRequest(new { error = "A request body is required." });

            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null || customer.Deleted) return NotFound();

            if (customer.IsSystemAccount)
                return BadRequest(new { error = $"System account '{customer.SystemName}' cannot be modified." });

            if (request.Email != null)
            {
                var email = request.Email.Trim();
                if (!CommonHelper.IsValidEmail(email))
                    return BadRequest(new { error = $"'{email}' is not a valid email address." });

                try
                {
                    //persisted immediately, and keeps the newsletter subscriptions of the old address
                    await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation: false);
                }
                catch (NopException ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }

            if (request.Username != null)
            {
                if (!_customerSettings.UsernamesEnabled)
                    return BadRequest(new { error = "Usernames are disabled on this store." });

                try
                {
                    await _customerRegistrationService.SetUsernameAsync(customer, request.Username);
                }
                catch (NopException ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }

            //null means "not supplied" and leaves the value alone; "" clears it
            if (request.FirstName != null) customer.FirstName = request.FirstName.Trim();
            if (request.LastName != null) customer.LastName = request.LastName.Trim();
            if (request.Phone != null) customer.Phone = request.Phone.Trim();
            if (request.AdminComment != null) customer.AdminComment = request.AdminComment;
            if (request.Active.HasValue) customer.Active = request.Active.Value;

            await _customerService.UpdateCustomerAsync(customer);

            if (!string.IsNullOrEmpty(request.Password))
            {
                //an administrator reset: the request is not validated against the old password.
                //The service looks the customer up by email, so it runs after the email change above
                var change = await _customerRegistrationService.ChangePasswordAsync(
                    new ChangePasswordRequest(customer.Email, validateRequest: false,
                        _customerSettings.DefaultPasswordFormat, request.Password));

                if (!change.Success)
                    return BadRequest(new { error = string.Join(" ", change.Errors) });
            }

            return Ok(customer.ToDto());
        }

        // DELETE api/rest/customers/guests
        [HttpDelete("guests")]
        public async Task<IActionResult> DeleteGuests()
        {
            //every deletable guest, including those with a shopping cart. Guests with orders,
            //reviews, blog comments or system accounts are never candidates, so they survive here
            //exactly as they do in the scheduled DeleteGuestsTask
            var deleted = await _customerService.DeleteGuestCustomersAsync(null, null, onlyWithoutShoppingCart: false);

            return Ok(new { deleted });
        }

        // DELETE api/rest/customers/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null || customer.Deleted) return NotFound();

            if (customer.IsSystemAccount)
                return BadRequest(new { error = $"System account '{customer.SystemName}' cannot be deleted." });

            await _customerService.DeleteCustomerAsync(customer);

            return NoContent();
        }
    }
}
