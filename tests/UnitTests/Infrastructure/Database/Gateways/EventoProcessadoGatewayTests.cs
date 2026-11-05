using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Database.Gateways;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infrastructure.Database.Gateways;

public class EventoProcessadoGatewayTests
{
    [Fact]
    public async Task AddAndExists_Work()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDbContext(options);
        var gateway = new EventoProcessadoGateway(context);
        var uow = new UnitOfWork(context);
        var id = Guid.NewGuid();

        // Act
        Assert.False(await gateway.ExisteEventIdAsync(id));
        await gateway.AddAsync(new EventoProcessado
        {
            EventId = id, Tipo = "T", ChaveNegocio = "K", ProcessadoEm = DateTimeOffset.UtcNow
        });
        await uow.SaveChangesAsync();

        // Assert
        Assert.True(await gateway.ExisteEventIdAsync(id));
        Assert.True(await gateway.ExisteChaveNegocioAsync("T", "K"));
    }
}
