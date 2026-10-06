namespace Nop.Plugin.Misc.Mcp.Dto;

/// <summary>
/// Represents a light-weight customer projection
/// </summary>
public record CustomerInfo
{
    public int Id { get; set; }

    public Guid CustomerGuid { get; set; }

    public string Email { get; set; }

    public string Username { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FullName { get; set; }

    public string Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string Company { get; set; }

    public string Phone { get; set; }

    public string VatNumber { get; set; }

    public int AffiliateId { get; set; }

    public int VendorId { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public bool IsSystemAccount { get; set; }

    public string SystemName { get; set; }

    public string AdminComment { get; set; }

    public int RegisteredInStoreId { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime LastActivityDateUtc { get; set; }

    public DateTime? LastLoginDateUtc { get; set; }

    public IList<string> Roles { get; set; } = new List<string>();
}