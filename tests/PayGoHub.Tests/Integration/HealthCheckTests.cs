using Xunit;

namespace PayGoHub.Tests.Integration;

[Trait("Category", "Integration")]
public class HealthCheckTests
{
    [Fact(Skip = "Requires running application")]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        // This test requires a running application
        // In CI, this is handled by the docker-compose based integration tests
        using var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5000");

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", content);
    }
}
