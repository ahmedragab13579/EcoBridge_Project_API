namespace EcoBridge.Application.Contracts;

public interface IRedisServices
{
    Task<T?> GetDataAsync<T>(string key);
    Task SetDataAsync<T>(string key, T data, TimeSpan? expirationTime = null);
    Task RemoveDataAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
}
