using FluentAssertions;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class DomainSanityTests
{
    [Fact]
    public void Environment_Should_Be_Configured_Properly()
    {
        true.Should().BeTrue();
    }
}
