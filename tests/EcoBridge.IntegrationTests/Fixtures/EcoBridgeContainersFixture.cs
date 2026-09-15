using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;
using Testcontainers.Redis;
using Xunit;

namespace EcoBridge.IntegrationTests.Fixtures;

public class EcoBridgeContainersFixture : IAsyncLifetime
{
    public MsSqlContainer SqlServerContainer { get; }
    public RedisContainer RedisContainer { get; }

    public EcoBridgeContainersFixture()
    {
        SqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("P@ssw0rd123!")
            .WithCleanUp(true)
            .Build();

        RedisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public string GetSqlConnectionString() => SqlServerContainer.GetConnectionString() + ";TrustServerCertificate=True;MultipleActiveResultSets=true;";
    public string GetRedisConnectionString() => RedisContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            SqlServerContainer.StartAsync(),
            RedisContainer.StartAsync()
        );
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            SqlServerContainer.DisposeAsync().AsTask(),
            RedisContainer.DisposeAsync().AsTask()
        );
    }
}
