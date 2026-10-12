using Xunit;

namespace EcoBridge.IntegrationTests;

[CollectionDefinition("EcoBridgeIntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<EcoBridge.IntegrationTests.Fixtures.CustomWebApplicationFactory>
{
}
