using EcoBridge.Domain.Enums;

namespace EcoBridge.Domain.Models.Mission;

public partial class DeliveryMission
{
    public bool IsClosed() =>
        CancelledAtUtc is not null || DeliveryTime is not null || Status == MissionStatus.Delivered || Status == MissionStatus.Cancelled || IsDeleted;

    public bool IsAssignedTo(Guid volunteerId) => VolunteerId == volunteerId;
}