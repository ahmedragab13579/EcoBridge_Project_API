using System;
using System.Threading.Tasks;
using EcoBridge.Application.Contracts;
using EcoBridge.IntegrationTests.Common;
using EcoBridge.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcoBridge.IntegrationTests.Caching;

public class RedisCacheIntegrationTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    private record SampleUserDto(Guid Id, string Name, string Role);

    [Fact]
    public async Task RedisCacheService_Should_Set_Get_And_Remove_Values_In_Real_Redis()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var cacheKey = $"test_user_{Guid.NewGuid()}";
        var sampleDto = new SampleUserDto(Guid.NewGuid(), "Integration User", "Donor");

        // Act - Set value
        await cacheService.SetAsync(cacheKey, sampleDto, TimeSpan.FromMinutes(5));

        // Assert - Get value
        var cached = await cacheService.GetAsync<SampleUserDto>(cacheKey);
        cached.Should().NotBeNull();
        cached!.Id.Should().Be(sampleDto.Id);
        cached.Name.Should().Be("Integration User");

        // Act - Remove value
        await cacheService.RemoveAsync(cacheKey);

        // Assert - Removed
        var removed = await cacheService.GetAsync<SampleUserDto>(cacheKey);
        removed.Should().BeNull();
    }

    [Fact]
    public async Task RedisRefreshTokenService_Should_Save_Validate_And_Revoke_Tokens_In_Redis()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
        var userId = Guid.NewGuid();

        // Act - Generate and save
        var token = await tokenService.GenerateAndSaveRefreshTokenAsync(userId);
        token.Should().NotBeNullOrWhiteSpace();

        // Assert - Validate
        var validatedUserId = await tokenService.ValidateRefreshTokenAsync(token);
        validatedUserId.Should().Be(userId);

        // Act - Revoke
        var revoked = await tokenService.RevokeRefreshTokenAsync(token);
        revoked.Should().BeTrue();

        // Assert - Validate after revocation
        var afterRevocation = await tokenService.ValidateRefreshTokenAsync(token);
        afterRevocation.Should().BeNull();
    }
}
