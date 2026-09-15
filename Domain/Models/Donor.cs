using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Enums;

namespace EcoBridge.Domain.Models;

public class Donor
{
    [Key]
    [ForeignKey(nameof(Account))]
    public Guid AccountId { get; protected set; }

    [Column(TypeName = "tinyint")]
    public UserType UserType { get; protected set; }

    [MaxLength(150)]
    public string? OrganizationName { get; protected set; }

    [MaxLength(255)]
    public string? Address { get; protected set; }

    public Account Account { get; set; } = null!;
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();

    protected Donor() { }

    public static Donor Create(Guid accountId, UserType userType, string? organizationName = null, string? address = null)
    {
        return new Donor
        {
            AccountId = accountId,
            UserType = userType,
            OrganizationName = organizationName,
            Address = address
        };
    }

    public void UpdateDetails(string? organizationName, string? address)
    {
        OrganizationName = organizationName;
        Address = address;
    }
}
