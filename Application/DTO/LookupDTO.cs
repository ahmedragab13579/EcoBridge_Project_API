namespace EcoBridge.Application.DTO;

public class LookupDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }

    public LookupDTO() { }

    public LookupDTO(int id, string name, string? code = null)
    {
        Id = id;
        Name = name;
        Code = code;
    }
}

public class ReviewReasonLookupDTO
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public string FromRole { get; set; } = string.Empty;
    public string ToRole { get; set; } = string.Empty;
}
