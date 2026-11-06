using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    public CustomWebApplicationFactory()
    {
        EnsureDockerHost();
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("estoque_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            _postgres.GetConnectionString() + ";Maximum Pool Size=15");
        Environment.SetEnvironmentVariable("Jwt__Funcionario__Issuer", TestJwt.Issuer);
        Environment.SetEnvironmentVariable("Jwt__Funcionario__Audience", TestJwt.Audience);
        Environment.SetEnvironmentVariable("Jwt__Funcionario__PublicKeyPem", TestJwt.PublicKeyPem);
        Environment.SetEnvironmentVariable("Jwt__Funcionario__RequireHttpsMetadata", "false");
        Environment.SetEnvironmentVariable("Jwt__Cliente__Issuer", TestJwt.Issuer);
        Environment.SetEnvironmentVariable("Jwt__Cliente__Audience", TestJwt.Audience);
        Environment.SetEnvironmentVariable("Jwt__Cliente__PublicKeyPem", TestJwt.PublicKeyPem);
        Environment.SetEnvironmentVariable("Jwt__Cliente__RequireHttpsMetadata", "false");
        Environment.SetEnvironmentVariable("ServiceAuth__Key", "integration-service-auth-key");
        Environment.SetEnvironmentVariable("PubSub__ProjectId", "integration");
        Environment.SetEnvironmentVariable("PubSub__UseInMemory", "true");
        Environment.SetEnvironmentVariable("PubSub__EnableWorkers", "false");
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var connectionString = _postgres.GetConnectionString() + ";Maximum Pool Size=15";
        var publicPem = TestJwt.PublicKeyPem;

        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
        builder.UseSetting("Jwt:Funcionario:Issuer", TestJwt.Issuer);
        builder.UseSetting("Jwt:Funcionario:Audience", TestJwt.Audience);
        builder.UseSetting("Jwt:Funcionario:PublicKeyPem", publicPem);
        builder.UseSetting("Jwt:Funcionario:RequireHttpsMetadata", "false");
        builder.UseSetting("Jwt:Cliente:Issuer", TestJwt.Issuer);
        builder.UseSetting("Jwt:Cliente:Audience", TestJwt.Audience);
        builder.UseSetting("Jwt:Cliente:PublicKeyPem", publicPem);
        builder.UseSetting("Jwt:Cliente:RequireHttpsMetadata", "false");
        builder.UseSetting("ServiceAuth:Key", "integration-service-auth-key");
        builder.UseSetting("PubSub:ProjectId", "integration");
        builder.UseSetting("PubSub:UseInMemory", "true");
        builder.UseSetting("PubSub:EnableWorkers", "false");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Jwt:Funcionario:Issuer"] = TestJwt.Issuer,
                ["Jwt:Funcionario:Audience"] = TestJwt.Audience,
                ["Jwt:Funcionario:PublicKeyPem"] = publicPem,
                ["Jwt:Funcionario:RequireHttpsMetadata"] = "false",
                ["Jwt:Cliente:Issuer"] = TestJwt.Issuer,
                ["Jwt:Cliente:Audience"] = TestJwt.Audience,
                ["Jwt:Cliente:PublicKeyPem"] = publicPem,
                ["Jwt:Cliente:RequireHttpsMetadata"] = "false",
                ["ServiceAuth:Key"] = "integration-service-auth-key",
                ["PubSub:ProjectId"] = "integration",
                ["PubSub:UseInMemory"] = "true",
                ["PubSub:EnableWorkers"] = "false"
            });
        });
    }

    private static void EnsureDockerHost()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DOCKER_HOST")))
        {
            return;
        }

        if (File.Exists("/var/run/docker.sock"))
        {
            Environment.SetEnvironmentVariable("DOCKER_HOST", "unix:///var/run/docker.sock");
        }
    }
}

[CollectionDefinition(nameof(IntegrationFixture))]
public class IntegrationFixture : ICollectionFixture<CustomWebApplicationFactory>;
