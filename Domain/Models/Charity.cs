using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Contracts;

namespace EcoBridge.Domain.Models;

public class Charity : ISoftDeletable
{
    [Key]
    [ForeignKey(nameof(Account))]
    public Guid AccountId { get; protected set; }

    [Required]
    [MaxLength(150)]
    public string OrganizationName { get; protected set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RegistrationNumber { get; protected set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; protected set; }

    public bool IsDeleted { get; set; } = false;

    [Column(TypeName = "datetime2")]
    public DateTime? DeletedAt { get; set; }

    public Account Account { get; set; } = null!;
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<DonationRequest> DonationRequests { get; set; } = new List<DonationRequest>();

    protected Charity() { }

    public static Charity Create(Guid accountId, string organizationName, string registrationNumber, string? address = null)
    {
        return new Charity
        {
            AccountId = accountId,
            OrganizationName = organizationName,
            RegistrationNumber = registrationNumber,
            Address = address,
            IsDeleted = false,
            DeletedAt = null
        };
    }

    public void UpdateOrganization(string organizationName, string registrationNumber, string? address)
    {
        OrganizationName = organizationName;
        RegistrationNumber = registrationNumber;
        Address = address;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}
