using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Contracts;

namespace EcoBridge.Domain.Models;

public class Volunteer : ISoftDeletable
{
    [Key]
    [ForeignKey(nameof(Account))]
    public Guid AccountId { get; protected set; }

    [MaxLength(100)]
    public string? VehicleDetails { get; protected set; }

    [MaxLength(50)]
    public string? VehicleType { get; protected set; }

    public bool IsAvailable { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    [Column(TypeName = "datetime2")]
    public DateTime? DeletedAt { get; set; }

    public Account Account { get; set; } = null!;
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<DeliveryMission> DeliveryMissions { get; set; } = new List<DeliveryMission>();

    protected Volunteer() { }

    public static Volunteer Create(Guid accountId, string? vehicleType = null, string? vehicleDetails = null)
    {
        return new Volunteer
        {
            AccountId = accountId,
            VehicleType = vehicleType,
            VehicleDetails = vehicleDetails,
            IsAvailable = true,
            IsDeleted = false,
            DeletedAt = null
        };
    }

    public void UpdateVehicle(string? vehicleType, string? vehicleDetails)
    {
        VehicleType = vehicleType;
        VehicleDetails = vehicleDetails;
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        IsAvailable = false;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        IsAvailable = true;
    }
}
