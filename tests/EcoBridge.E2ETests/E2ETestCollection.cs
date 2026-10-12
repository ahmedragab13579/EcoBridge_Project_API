using Xunit;

namespace EcoBridge.E2ETests;

[CollectionDefinition("EcoBridgeE2ETests")]
public class E2ETestCollection : ICollectionFixture<EcoBridge.E2ETests.Fixtures.CustomE2EWebApplicationFactory>
{
}
