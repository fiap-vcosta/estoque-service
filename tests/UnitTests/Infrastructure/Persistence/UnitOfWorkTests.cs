using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace UnitTests.Infrastructure.Persistence;

public class UnitOfWorkTests
{
    private static DbContextOptions<AppDbContext> Options() =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    [Fact]
    public async Task SaveChangesAsync_Persists()
    {
        // Arrange
        await using var context = new AppDbContext(Options());
        var uow = new UnitOfWork(context);
        context.OrdensDescartadas.Add(new OrdemDescartada { OrdemServicoId = 1, DescartadaEm = DateTimeOffset.UtcNow });

        // Act
        await uow.SaveChangesAsync();

        // Assert
        Assert.Equal(1, await context.OrdensDescartadas.CountAsync());
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_PersistsAction()
    {
        // Arrange
        await using var context = new AppDbContext(Options());
        var uow = new UnitOfWork(context);

        // Act
        await uow.ExecuteInTransactionAsync(async ct =>
        {
            context.OrdensDescartadas.Add(new OrdemDescartada { OrdemServicoId = 2, DescartadaEm = DateTimeOffset.UtcNow });
            await Task.CompletedTask;
        });

        // Assert
        Assert.Equal(1, await context.OrdensDescartadas.CountAsync());
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenActionThrows_Propagates()
    {
        // Arrange
        await using var context = new AppDbContext(Options());
        var uow = new UnitOfWork(context);

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            uow.ExecuteInTransactionAsync(_ => throw new InvalidOperationException("boom")));
    }
}
