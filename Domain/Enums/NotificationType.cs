namespace EcoBridge.Domain.Enums;

public enum NotificationType : byte
{
    MissionAvailable = 0,
    MissionAssigned = 1,
    MissionTaken = 2,
    DeliveryCompleted = 3,
    PenaltyApplied = 4,
    DonationStatusChanged = 5
}
