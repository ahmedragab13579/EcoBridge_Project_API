namespace EcoBridge.Application.Extensions;

public static class CloudinaryExtensions
{
    private const string DefaultCloudName = "pbdbexm9";

    /// <summary>
    /// Ensures that an image path or public ID is resolved to an absolute Cloudinary CDN URL.
    /// </summary>
    public static string? ToCloudinaryUrl(this string? imagePath, string cloudName = DefaultCloudName)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return null;

        if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return imagePath;
        }

        var cleanPath = imagePath.TrimStart('/');
        return $"https://res.cloudinary.com/{cloudName}/image/upload/{cleanPath}";
    }
}
