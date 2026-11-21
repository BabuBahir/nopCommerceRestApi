using Nop.Core.Domain.Common;

namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a light-weight address projection
/// </summary>
public record AddressInfo
{
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string Company { get; set; }

    public int? CountryId { get; set; }

    public int? StateProvinceId { get; set; }

    public string County { get; set; }

    public string City { get; set; }

    public string Address1 { get; set; }

    public string Address2 { get; set; }

    public string ZipPostalCode { get; set; }

    public string PhoneNumber { get; set; }

    public string FaxNumber { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Creates an AddressInfo instance from an Address entity
    /// </summary>
    /// <param name="address">Address entity</param>
    /// <returns>The AddressInfo instance, or null when the entity is null</returns>
    public static AddressInfo FromEntity(Address address)
    {
        if (address is null)
            return null;

        return new AddressInfo
        {
            Id = address.Id,
            FirstName = address.FirstName,
            LastName = address.LastName,
            Email = address.Email,
            Company = address.Company,
            CountryId = address.CountryId,
            StateProvinceId = address.StateProvinceId,
            County = address.County,
            City = address.City,
            Address1 = address.Address1,
            Address2 = address.Address2,
            ZipPostalCode = address.ZipPostalCode,
            PhoneNumber = address.PhoneNumber,
            FaxNumber = address.FaxNumber,
            CreatedOnUtc = address.CreatedOnUtc
        };
    }
}