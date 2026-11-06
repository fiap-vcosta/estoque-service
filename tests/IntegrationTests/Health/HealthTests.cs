using System.Net;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.Health;

[Collection(nameof(IntegrationFixture))]
public class HealthTests
{
    private readonly HttpClient _client;

    public HealthTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Health_ReturnsOk()
    {
        // Arrange / Act
        var response = await _client.GetAsync("/estoque/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
