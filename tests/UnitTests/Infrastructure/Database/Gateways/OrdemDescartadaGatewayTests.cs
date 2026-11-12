using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Database.Gateways;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infrastructure.Database.Gateways;

public class OrdemDescartadaGatewayTests
{
    [Fact]
    public async Task AddAndExists_Work()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDbContext(options);
        var gateway = new OrdemDescartadaGateway(context);
        var uow = new UnitOfWork(context);

        // Act
        Assert.False(await gateway.ExisteAsync(Guid.Parse("00000000-0000-0000-0000-000000000010")));
        await gateway.AddAsync(new OrdemDescartada { OrdemServicoId = Guid.Parse("00000000-0000-0000-0000-000000000010"), DescartadaEm = DateTimeOffset.UtcNow });
        await uow.SaveChangesAsync();

        // Assert
        Assert.True(await gateway.ExisteAsync(Guid.Parse("00000000-0000-0000-0000-000000000010")));
    }
}
