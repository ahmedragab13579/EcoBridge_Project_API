using System.Net.Http.Headers;
using EcoBridge.Domain.Enums;
using EcoBridge.Infrastructure.Data;
using EcoBridge.IntegrationTests.Fixtures;
using EcoBridge.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcoBridge.IntegrationTests.Common;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected HttpClient CreateAuthenticatedClient(Guid userId, string email, UserRole role)
    {
        var client = Factory.CreateClient();
        var token = AuthTestHelper.GenerateJwtToken(userId, email, role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected async Task ExecuteDbContextAsync(Func<EcoBridgeDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EcoBridgeDbContext>();
        await action(db);
    }

    protected async Task<T> ExecuteDbContextAsync<T>(Func<EcoBridgeDbContext, Task<T>> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EcoBridgeDbContext>();
        return await action(db);
    }
}
