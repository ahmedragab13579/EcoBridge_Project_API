using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Behaviors;

public class CachingPipelineBehaviorTests
{
    private sealed record CacheableRequest(string CacheKey, int CacheDurationMinutes = 5) : ICacheableQuery;
    private sealed record PlainRequest(string Value);

    [Fact]
    public async Task Handle_Should_Return_Cached_Response_Without_Invoking_Next()
    {
        var cache = new Mock<ICacheService>();
        cache.Setup(x => x.GetAsync<string>("donation:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("cached");
        var behavior = new CachingPipelineBehavior<CacheableRequest, string>(
            cache.Object,
            NullLogger<CachingPipelineBehavior<CacheableRequest, string>>.Instance);
        var nextCalled = false;

        var result = await behavior.Handle(
            new CacheableRequest("donation:1"),
            () =>
            {
                nextCalled = true;
                return Task.FromResult("fresh");
            },
            CancellationToken.None);

        result.Should().Be("cached");
        nextCalled.Should().BeFalse();
        cache.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Invoke_Next_And_Set_Cache_On_Miss()
    {
        var cache = new Mock<ICacheService>();
        cache.Setup(x => x.GetAsync<string>("donation:2", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var behavior = new CachingPipelineBehavior<CacheableRequest, string>(
            cache.Object,
            NullLogger<CachingPipelineBehavior<CacheableRequest, string>>.Instance);

        var result = await behavior.Handle(
            new CacheableRequest("donation:2", 10),
            () => Task.FromResult("fresh"),
            CancellationToken.None);

        result.Should().Be("fresh");
        cache.Verify(x => x.SetAsync(
            "donation:2",
            "fresh",
            It.Is<TimeSpan?>(ttl => ttl == TimeSpan.FromMinutes(10)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Pass_Through_NonCacheable_Request()
    {
        var cache = new Mock<ICacheService>();
        var behavior = new CachingPipelineBehavior<PlainRequest, string>(
            cache.Object,
            NullLogger<CachingPipelineBehavior<PlainRequest, string>>.Instance);

        var result = await behavior.Handle(
            new PlainRequest("value"),
            () => Task.FromResult("response"),
            CancellationToken.None);

        result.Should().Be("response");
        cache.Verify(x => x.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
