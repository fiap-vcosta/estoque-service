using Api.Controllers.SystemApi.ItemEstoque;
using Application.UseCases.ItemEstoque.Queries.GetItemEstoqueById;
using Application.UseCases.ItemEstoque.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Api.Controllers.SystemApi.ItemEstoque;

public class ItemEstoqueSystemControllerTests
{
    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetItemEstoqueByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ItemEstoqueResponse?)null);
        var sut = new ItemEstoqueSystemController(mediator.Object);

        // Act
        var result = await sut.GetById(1, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsOkWithoutBalances()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetItemEstoqueByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ItemEstoqueResponse
            {
                Id = 1, Codigo = "C", Nome = "N", UnidadeMedida = UnidadeMedida.Kg,
                Tipo = ItemTipo.Peca, PrecoVenda = 9, Saldo = 1, SaldoReservado = 1
            });
        var sut = new ItemEstoqueSystemController(mediator.Object);

        // Act
        var result = await sut.GetById(1, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
