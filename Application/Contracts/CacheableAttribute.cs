namespace EcoBridge.Application.Contracts;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class CacheableAttribute : Attribute
{
    public int TTLMinutes { get; set; } = 15;
    public string? CustomKeyPrefix { get; set; }

    public CacheableAttribute() { }

    public CacheableAttribute(int ttlMinutes)
    {
        TTLMinutes = ttlMinutes;
    }
}

public interface ICacheableQuery
{
    string CacheKey { get; }
    int CacheDurationMinutes => 15;
}
