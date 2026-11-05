using System.Net;
using System.Net.Http.Json;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.Cadastros;

[Collection(nameof(IntegrationFixture))]
public class ItemEstoqueCadastrosTests
{
    private readonly HttpClient _client;

    public ItemEstoqueCadastrosTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        AuthHelper.AuthenticateAsStaff(_client);
    }

    [Fact]
    public async Task CrudAndEntrada_Works()
    {
        // Arrange
        var create = new
        {
            codigo = "FILTRO-1",
            tipo = "Peca",
            nome = "Filtro",
            unidadeMedida = "Unidade",
            precoVenda = 25.5,
            saldo = 10,
            saldoReservado = 0
        };

        // Act
        var created = await _client.PostAsJsonAsync("/estoque/itens", create);
        var body = await created.Content.ReadFromJsonAsync<ItemDto>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(10, body.Saldo);

        var entrada = await _client.PostAsJsonAsync($"/estoque/itens/{body.Id}/registrar-entrada", new { quantidade = 5 });
        Assert.Equal(HttpStatusCode.OK, entrada.StatusCode);
        var after = await entrada.Content.ReadFromJsonAsync<ItemDto>();
        Assert.Equal(15, after!.Saldo);
    }

    private sealed class ItemDto
    {
        public int Id { get; set; }
        public decimal Saldo { get; set; }
        public decimal SaldoReservado { get; set; }
    }
}
