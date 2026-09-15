namespace EcoBridge.Application.Contracts;

public class UploadImageResponse
{
    public string PublicId { get; set; } = string.Empty;
    public string? Url { get; set; }
}
