using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.SystemApi;

[Collection(nameof(IntegrationFixture))]
public class ItemEstoqueSystemApiTests
{
    private readonly CustomWebApplicationFactory _factory;

    public ItemEstoqueSystemApiTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetById_WithServiceKey_ReturnsSnapshotWithoutBalances()
    {
        // Arrange
        var staff = _factory.CreateClient();
        AuthHelper.AuthenticateAsStaff(staff);
        var created = await staff.PostAsJsonAsync("/estoque/itens", new
        {
            codigo = "SYS-1",
            tipo = "Peca",
            nome = "Peca System",
            unidadeMedida = "Unidade",
            precoVenda = 10,
            saldo = 3,
            saldoReservado = 1
        });
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var system = _factory.CreateClient();
        system.DefaultRequestHeaders.Add("X-Service-Key", "integration-service-auth-key");

        // Act
        var response = await system.GetAsync($"/system/estoque/itens/{id}");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("SYS-1", json.GetProperty("codigo").GetString());
        Assert.False(json.TryGetProperty("saldo", out _));
        Assert.False(json.TryGetProperty("precoVenda", out _));
    }

    [Fact]
    public async Task GetById_WithoutServiceKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/system/estoque/itens/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
