namespace EcoBridge.Application.DTO.Statics.Lookups;

public class CountryDTO
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public List<GovernorateDTO> Governorates { get; set; } = new();
}

public class GovernorateDTO
{
    public int Id { get; set; }
    public int CountryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public List<CityDTO> Cities { get; set; } = new();
}

public class CityDTO
{
    public int Id { get; set; }
    public int GovernorateId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

public class ReviewReasonDTO
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public string FromRole { get; set; } = string.Empty;
    public string ToRole { get; set; } = string.Empty;
}
