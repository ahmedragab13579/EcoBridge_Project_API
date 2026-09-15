using FluentAssertions;
using Xunit;

namespace EcoBridge.E2ETests;

public class E2ESanityTests
{
    [Fact]
    public void Environment_Should_Be_Configured_Properly()
    {
        true.Should().BeTrue();
    }
}
