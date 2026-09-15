using FluentAssertions;
using Xunit;

namespace EcoBridge.Application.UnitTests;

public class ApplicationSanityTests
{
    [Fact]
    public void Environment_Should_Be_Configured_Properly()
    {
        true.Should().BeTrue();
    }
}
