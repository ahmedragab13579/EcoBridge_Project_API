using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EcoBridge.Domain.Enums;
using EcoBridge.Infrastructure.Data;
using EcoBridge.E2ETests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace EcoBridge.E2ETests.Common;

[Collection("EcoBridgeE2ETests")]
public abstract class BaseE2ETest
{
    private const string SecretKey = "TestSuperSecretKeyForE2ETesting20261234567890!";
    private const string Issuer = "EcoBridgeAPI";
    private const string Audience = "EcoBridgeClients";

    protected readonly CustomE2EWebApplicationFactory Factory;
    protected readonly HttpClient AnonymousClient;

    protected BaseE2ETest(CustomE2EWebApplicationFactory factory)
    {
        Factory = factory;
        AnonymousClient = factory.CreateClient();
        AnonymousClient.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.{Random.Shared.Next(1, 250)}.{Random.Shared.Next(1, 250)}");
        var csrf = Guid.NewGuid().ToString();
        AnonymousClient.DefaultRequestHeaders.Add("Cookie", $"csrfToken={csrf}");
        AnonymousClient.DefaultRequestHeaders.Add("X-CSRF-Token", csrf);
    }

    protected HttpClient CreateAuthenticatedClient(Guid userId, string email, UserRole role)
    {
        var client = Factory.CreateClient();
        var token = GenerateJwtToken(userId, email, role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.{Random.Shared.Next(1, 250)}.{Random.Shared.Next(1, 250)}");
        var csrf = Guid.NewGuid().ToString();
        client.DefaultRequestHeaders.Add("Cookie", $"csrfToken={csrf}");
        client.DefaultRequestHeaders.Add("X-CSRF-Token", csrf);
        return client;
    }

    protected static string GenerateJwtToken(Guid userId, string email, UserRole role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
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
