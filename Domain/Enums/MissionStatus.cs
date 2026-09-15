namespace EcoBridge.Domain.Enums;

public enum MissionStatus : byte
{
    SearchingForVolunteer = 0,
    Accepted = 1,
    InTransit = 2,
    Delivered = 3,
    Cancelled = 4
}
