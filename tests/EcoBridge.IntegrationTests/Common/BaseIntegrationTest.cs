using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Infrastructure.Data;
using EcoBridge.IntegrationTests.Fixtures;
using EcoBridge.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Xunit;

namespace EcoBridge.IntegrationTests.Common;

[Collection("EcoBridgeIntegrationTests")]
public abstract class BaseIntegrationTest
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.{Random.Shared.Next(1, 250)}.{Random.Shared.Next(1, 250)}");
    }

    protected Task ResetDatabaseAsync() => Factory.ResetDatabaseAsync();

    protected HttpClient CreateAuthenticatedClient(Guid userId, string email, UserRole role)
    {
        var client = Factory.CreateClient();
        var token = AuthTestHelper.GenerateJwtToken(userId, email, role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.{Random.Shared.Next(1, 250)}.{Random.Shared.Next(1, 250)}");
        var csrf = Guid.NewGuid().ToString();
        client.DefaultRequestHeaders.Add("Cookie", $"csrfToken={csrf}");
        client.DefaultRequestHeaders.Add("X-CSRF-Token", csrf);
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
