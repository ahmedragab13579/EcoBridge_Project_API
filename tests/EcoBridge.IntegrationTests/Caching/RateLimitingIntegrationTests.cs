using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EcoBridge.Application.DTO;
using EcoBridge.IntegrationTests.Common;
using EcoBridge.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit;

namespace EcoBridge.IntegrationTests.Caching;

public class RateLimitingIntegrationTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task SecurityFixedWindow_Should_Return_429_When_Limit_Exceeded()
    {
        // Arrange
        var request = new LoginRequestDTO
        {
            Email = "ratelimit_test@example.com",
            Password = "Password123!"
        };

        HttpResponseMessage? lastResponse = null;

        // Act - Trigger up to 10 requests rapidly (PermitLimit = 5)
        for (int i = 0; i < 8; i++)
        {
            lastResponse = await Client.PostAsJsonAsync("/api/auth/login", request);
            if (lastResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                break;
            }
        }

        // Assert - The rate limiter policy should have triggered 429
        lastResponse.Should().NotBeNull();
        lastResponse!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
