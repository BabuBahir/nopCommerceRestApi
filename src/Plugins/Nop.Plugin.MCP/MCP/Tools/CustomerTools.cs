using System.ComponentModel;
using ModelContextProtocol.Server;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Mcp.Areas.Admin.Models;
using Nop.Plugin.Misc.Mcp.Dto;
using Nop.Services.Common;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.Mcp.MCP.Tools;

/// <summary>
/// Represents the customer-related MCP tools
/// </summary>
[McpServerToolType]
public class CustomerTools : BaseMcpTool
{
    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly IAddressService _addressService;

    #endregion

    #region Ctor

    public CustomerTools(ICustomerService customerService,
        IAddressService addressService,
        McpSettings mcpSettings)
        : base(mcpSettings)
    {
        _customerService = customerService;
        _addressService = addressService;
    }

    #endregion

    #region Tools

    /// <summary>
    /// Gets a single customer by its identifier
    /// </summary>
    [McpServerTool(Name = "get_customer_by_id")]
    [Description("Returns the detail of a single customer (including roles) by identifier, or null when not found.")]
    public async Task<CustomerInfo> GetCustomerByIdAsync(
        [Description("The customer identifier.")] int customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);

        if (customer is null || customer.Deleted)
            return null;

        return await PrepareCustomerInfoAsync(customer);
    }

    /// <summary>
    /// Searches for customers
    /// </summary>
    [McpServerTool(Name = "get_customers")]
    [Description("Returns a paginated list of customers, optionally filtered by email, name, phone, role and activity state.")]
    public async Task<PagedResult<CustomerInfo>> GetCustomersAsync(
        [Description("Search by the customer email address.")] string email = null,
        [Description("Search by the customer first name.")] string firstName = null,
        [Description("Search by the customer last name.")] string lastName = null,
        [Description("Search by the customer phone number.")] string phone = null,
        [Description("Only return customers that have this role identifier (e.g. 3 for registered). 0 or null to skip.")] int? customerRoleId = null,
        [Description("Only return customers created on or after this UTC timestamp.")] DateTime? createdFromUtc = null,
        [Description("Only return customers created on or before this UTC timestamp.")] DateTime? createdToUtc = null,
        [Description("A value indicating whether to load only active customers. Null loads all.")] bool? active = null,
        [Description("Zero-based page index.")] int pageIndex = 0,
        [Description("Page size. Cannot exceed the configured maximum.")] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
        var size = NormalizePageSize(pageSize);
        var roleIds = customerRoleId is > 0 ? new[] { customerRoleId.Value } : null;

        var customers = await _customerService.GetAllCustomersAsync(
            createdFromUtc: createdFromUtc,
            createdToUtc: createdToUtc,
            customerRoleIds: roleIds,
            email: string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            firstName: string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim(),
            lastName: string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim(),
            phone: string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            isActive: active,
            pageIndex: pageIndex < 0 ? 0 : pageIndex,
            pageSize: size);

        var items = new List<CustomerInfo>(customers.Count);
        foreach (var customer in customers)
            items.Add(await PrepareCustomerInfoAsync(customer));

        return new PagedResult<CustomerInfo>
        {
            TotalCount = customers.TotalCount,
            PageIndex = customers.PageIndex,
            PageSize = customers.PageSize,
            TotalPages = customers.TotalPages,
            Items = items
        };
    }

    /// <summary>
    /// Gets all addresses of a customer
    /// </summary>
    [McpServerTool(Name = "get_customer_addresses")]
    [Description("Returns all addresses mapped to a customer by identifier.")]
    public async Task<IList<AddressInfo>> GetCustomerAddressesAsync(
        [Description("The customer identifier.")] int customerId,
        CancellationToken cancellationToken = default)
    {
        var addresses = await _customerService.GetAddressesByCustomerIdAsync(customerId);

        return addresses.Select(AddressInfo.FromEntity).ToList();
    }

    /// <summary>
    /// Creates a customer
    /// </summary>
    [McpServerTool(Name = "create_customer")]
    [Description("Creates a new customer with the given details. The gender defaults to male (M) unless a female (F) gender is requested. When no customer role is specified the Guest role is assigned; when a requested role is not available the Guest role is assigned instead. No password is set, so the customer cannot sign in until a password is assigned in the admin area. Write tool; only available when write tools are enabled.")]
    public async Task<CustomerInfo> CreateCustomerAsync(
        [Description("The customer email address.")] string email = null,
        [Description("The customer username.")] string username = null,
        [Description("The customer first name.")] string firstName = null,
        [Description("The customer last name.")] string lastName = null,
        [Description("The customer gender (M or F). Defaults to M (male) when omitted.")] string gender = null,
        [Description("The customer date of birth.")] DateTime? dateOfBirth = null,
        [Description("The customer company.")] string company = null,
        [Description("The customer phone number.")] string phone = null,
        [Description("The customer role to assign, by name or system name, e.g. 'Guest', 'Registered', 'Vendors' or a custom role. When omitted the Guest role is assigned.")] string customerRole = null,
        [Description("A value indicating whether the customer is active. Defaults to true.")] bool active = true,
        [Description("The customer street address.")] string streetAddress = null,
        [Description("The customer street address (second line).")] string streetAddress2 = null,
        [Description("The customer zip/postal code.")] string zipPostalCode = null,
        [Description("The customer city.")] string city = null,
        [Description("The customer country identifier. 0 to skip.")] int countryId = 0,
        [Description("The customer state/province identifier. 0 to skip.")] int stateProvinceId = 0,
        [Description("The customer admin comment.")] string adminComment = null,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);

        var role = string.IsNullOrWhiteSpace(customerRole)
            ? guestRole
            : await ResolveCustomerRoleAsync(customerRole);

        var customer = new Customer
        {
            Email = email,
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            Gender = NormalizeGender(gender),
            DateOfBirth = dateOfBirth,
            Company = company,
            Phone = phone,
            Active = active,
            StreetAddress = streetAddress,
            StreetAddress2 = streetAddress2,
            ZipPostalCode = zipPostalCode,
            City = city,
            CountryId = countryId,
            StateProvinceId = stateProvinceId,
            AdminComment = adminComment,
            CreatedOnUtc = DateTime.UtcNow,
            LastActivityDateUtc = DateTime.UtcNow
        };

        await _customerService.InsertCustomerAsync(customer);

        if (role is null)
        {
            await AssignCustomerRoleAsync(customer, guestRole);
            throw new Exception($"No such customer role '{customerRole}' is available. The Guest role was assigned instead.");
        }

        await AssignCustomerRoleAsync(customer, role);

        return await PrepareCustomerInfoAsync(customer);
    }

    /// <summary>
    /// Updates a customer
    /// </summary>
    [McpServerTool(Name = "update_customer")]
    [Description("Updates the fields of an existing customer. Parameters that are omitted or left at their default value are left unchanged, except gender (defaults to male M unless a female F gender is requested) and the customer role (defaults to Guest; a requested role that is not available falls back to Guest). Write tool; only available when write tools are enabled.")]
    public async Task<CustomerInfo> UpdateCustomerAsync(
        [Description("The customer identifier to update.")] int customerId,
        [Description("The customer email address.")] string email = null,
        [Description("The customer username.")] string username = null,
        [Description("The customer first name. Pass an empty string to clear it.")] string firstName = null,
        [Description("The customer last name. Pass an empty string to clear it.")] string lastName = null,
        [Description("The customer gender (M or F). Defaults to M (male) when omitted.")] string gender = null,
        [Description("The customer date of birth.")] DateTime? dateOfBirth = null,
        [Description("The customer company.")] string company = null,
        [Description("The customer phone number.")] string phone = null,
        [Description("The customer role to assign, by name or system name, e.g. 'Guest', 'Registered', 'Vendors' or a custom role. When omitted the Guest role is assigned.")] string customerRole = null,
        [Description("A value indicating whether the customer is active. Null leaves it unchanged.")] bool? active = null,
        [Description("The customer street address.")] string streetAddress = null,
        [Description("The customer street address (second line).")] string streetAddress2 = null,
        [Description("The customer zip/postal code.")] string zipPostalCode = null,
        [Description("The customer city.")] string city = null,
        [Description("The customer country identifier. 0 or null to leave unchanged.")] int? countryId = null,
        [Description("The customer state/province identifier. 0 or null to leave unchanged.")] int? stateProvinceId = null,
        [Description("The customer admin comment.")] string adminComment = null,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        var customer = await _customerService.GetCustomerByIdAsync(customerId);

        if (customer is null || customer.Deleted)
            return null;

        var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);

        var role = string.IsNullOrWhiteSpace(customerRole)
            ? guestRole
            : await ResolveCustomerRoleAsync(customerRole);

        if (email is not null)
            customer.Email = email;
        if (username is not null)
            customer.Username = username;
        if (firstName is not null)
            customer.FirstName = firstName;
        if (lastName is not null)
            customer.LastName = lastName;
        customer.Gender = NormalizeGender(gender);
        if (dateOfBirth is not null)
            customer.DateOfBirth = dateOfBirth;
        if (company is not null)
            customer.Company = company;
        if (phone is not null)
            customer.Phone = phone;
        if (active is not null)
            customer.Active = active.Value;
        if (streetAddress is not null)
            customer.StreetAddress = streetAddress;
        if (streetAddress2 is not null)
            customer.StreetAddress2 = streetAddress2;
        if (zipPostalCode is not null)
            customer.ZipPostalCode = zipPostalCode;
        if (city is not null)
            customer.City = city;
        if (countryId is > 0)
            customer.CountryId = countryId.Value;
        if (stateProvinceId is > 0)
            customer.StateProvinceId = stateProvinceId.Value;
        if (adminComment is not null)
            customer.AdminComment = adminComment;

        await _customerService.UpdateCustomerAsync(customer);

        if (role is null)
        {
            await AssignCustomerRoleAsync(customer, guestRole);
            throw new Exception($"No such customer role '{customerRole}' is available. The Guest role was assigned instead.");
        }

        await AssignCustomerRoleAsync(customer, role);

        return await PrepareCustomerInfoAsync(customer);
    }

    /// <summary>
    /// Adds an address to a customer
    /// </summary>
    [McpServerTool(Name = "add_customer_address")]
    [Description("Creates a new address record and attaches it to an existing customer. Write tool; only available when write tools are enabled.")]
    public async Task<AddressInfo> AddCustomerAddressAsync(
        [Description("The customer identifier the address will be attached to.")] int customerId,
        [Description("The address first name.")] string firstName = null,
        [Description("The address last name.")] string lastName = null,
        [Description("The address email address.")] string email = null,
        [Description("The address company.")] string company = null,
        [Description("The address country identifier. 0 to skip.")] int countryId = 0,
        [Description("The address state/province identifier. 0 to skip.")] int stateProvinceId = 0,
        [Description("The address county.")] string county = null,
        [Description("The address city.")] string city = null,
        [Description("The first address line.")] string address1 = null,
        [Description("The second address line.")] string address2 = null,
        [Description("The address zip/postal code.")] string zipPostalCode = null,
        [Description("The address phone number.")] string phoneNumber = null,
        [Description("The address fax number.")] string faxNumber = null,
        CancellationToken cancellationToken = default)
    {
        EnsureWriteToolsEnabled();

        var customer = await _customerService.GetCustomerByIdAsync(customerId);

        if (customer is null || customer.Deleted)
            return null;

        var address = new Address
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Company = company,
            CountryId = countryId > 0 ? countryId : null,
            StateProvinceId = stateProvinceId > 0 ? stateProvinceId : null,
            County = county,
            City = city,
            Address1 = address1,
            Address2 = address2,
            ZipPostalCode = zipPostalCode,
            PhoneNumber = phoneNumber,
            FaxNumber = faxNumber,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _addressService.InsertAddressAsync(address);
        await _customerService.InsertCustomerAddressAsync(customer, address);

        return AddressInfo.FromEntity(address);
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Prepares a light-weight customer projection
    /// </summary>
    protected async Task<CustomerInfo> PrepareCustomerInfoAsync(Customer customer)
    {
        var info = new CustomerInfo
        {
            Id = customer.Id,
            CustomerGuid = customer.CustomerGuid,
            Email = customer.Email,
            Username = customer.Username,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = await _customerService.GetCustomerFullNameAsync(customer),
            Gender = customer.Gender,
            DateOfBirth = customer.DateOfBirth,
            Company = customer.Company,
            Phone = customer.Phone,
            VatNumber = customer.VatNumber,
            AffiliateId = customer.AffiliateId,
            VendorId = customer.VendorId,
            Active = customer.Active,
            Deleted = customer.Deleted,
            IsSystemAccount = customer.IsSystemAccount,
            SystemName = customer.SystemName,
            AdminComment = customer.AdminComment,
            RegisteredInStoreId = customer.RegisteredInStoreId,
            CreatedOnUtc = customer.CreatedOnUtc,
            LastActivityDateUtc = customer.LastActivityDateUtc,
            LastLoginDateUtc = customer.LastLoginDateUtc
        };

        var roles = await _customerService.GetCustomerRolesAsync(customer, showHidden: false);
        info.Roles = roles.Select(role => role.Name).ToList();

        return info;
    }

    /// <summary>
    /// Resolves a customer role by its name or system name; returns null when the role is not available
    /// </summary>
    protected async Task<CustomerRole> ResolveCustomerRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return null;

        var roles = await _customerService.GetAllCustomerRolesAsync(showHidden: false);

        return roles.FirstOrDefault(candidate =>
            candidate.Name.Equals(role.Trim(), StringComparison.InvariantCultureIgnoreCase) ||
            candidate.SystemName.Equals(role.Trim(), StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Adds the customer-role mapping when the customer does not have the role yet
    /// </summary>
    protected async Task AssignCustomerRoleAsync(Customer customer, CustomerRole role)
    {
        if (role is null)
            return;

        var existingRoles = await _customerService.GetCustomerRolesAsync(customer, showHidden: false);
        if (existingRoles.Any(item => item.Id == role.Id))
            return;

        await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping
        {
            CustomerId = customer.Id,
            CustomerRoleId = role.Id
        });
    }

    /// <summary>
    /// Normalizes the gender to M (male) by default, or F (female) when requested
    /// </summary>
    protected static string NormalizeGender(string gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
            return "M";

        var normalized = gender.Trim();

        return normalized.Equals("F", StringComparison.InvariantCultureIgnoreCase) ||
               normalized.Equals("Female", StringComparison.InvariantCultureIgnoreCase)
            ? "F"
            : "M";
    }

    #endregion
}