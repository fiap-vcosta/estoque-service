using Api.Contracts.Validation;
using Api.Controllers.Staff.ItemEstoque;
using Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;
using Api.Controllers.Staff.ItemEstoque.RegistrarEntradaEstoque;
using Api.Controllers.Staff.ItemEstoque.UpdateItemEstoque;
using Api.Presenters.ItemEstoque;
using Application.UseCases.ItemEstoque.Commands.CreateItemEstoque;
using Application.UseCases.ItemEstoque.Commands.DeleteItemEstoque;
using Application.UseCases.ItemEstoque.Commands.RegistrarEntradaEstoque;
using Application.UseCases.ItemEstoque.Commands.UpdateItemEstoque;
using Application.UseCases.ItemEstoque.Queries.GetAllItensEstoque;
using Application.UseCases.ItemEstoque.Queries.GetItemEstoqueById;
using Application.UseCases.ItemEstoque.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Api.Controllers.Staff.ItemEstoque;

public class ItemEstoqueControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IValidator<CreateItemEstoqueRequest>> _create = new();
    private readonly Mock<IValidator<UpdateItemEstoqueRequest>> _update = new();
    private readonly Mock<IValidator<RegistrarEntradaEstoqueRequest>> _entrada = new();
    private readonly ItemEstoqueController _sut;

    public ItemEstoqueControllerTests()
    {
        _sut = new ItemEstoqueController(_mediator.Object, new ItemEstoquePresenter(), _create.Object, _update.Object, _entrada.Object);
    }

    private static ItemEstoqueResponse Resp() => new()
    {
        Id = 1, Codigo = "A", Tipo = ItemTipo.Peca, Nome = "N", UnidadeMedida = UnidadeMedida.Unidade,
        PrecoVenda = 10, Saldo = 5, SaldoReservado = 0
    };

    private static CreateItemEstoqueRequest EmptyCreate() => new()
    {
        Tipo = default,
        UnidadeMedida = default,
        PrecoVenda = default,
        Saldo = default,
        SaldoReservado = default
    };

    private static UpdateItemEstoqueRequest EmptyUpdate() => new()
    {
        Tipo = default,
        UnidadeMedida = default,
        PrecoVenda = default
    };

    [Fact]
    public async Task Create_WhenInvalid_ReturnsBadRequest()
    {
        // Arrange
        var vr = new ValidationResult();
        vr.Errors.Add("x");
        _create.Setup(v => v.Validate(It.IsAny<CreateItemEstoqueRequest>())).Returns(vr);

        // Act
        var result = await _sut.Create(EmptyCreate());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsCreated()
    {
        // Arrange
        _create.Setup(v => v.Validate(It.IsAny<CreateItemEstoqueRequest>())).Returns(new ValidationResult());
        _mediator.Setup(m => m.Send(It.IsAny<CreateItemEstoqueCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Resp());

        // Act
        var result = await _sut.Create(new CreateItemEstoqueRequest
        {
            Codigo = "A",
            Nome = "N",
            Tipo = ItemTipo.Peca,
            UnidadeMedida = UnidadeMedida.Unidade,
            PrecoVenda = 1,
            Saldo = 1,
            SaldoReservado = 0
        });

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<GetItemEstoqueByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync((ItemEstoqueResponse?)null);

        // Act
        var result = await _sut.GetById(9);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsOk()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<GetItemEstoqueByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Resp());

        // Act
        var result = await _sut.GetById(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<GetAllItensEstoqueQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Resp()]);

        // Act
        var result = await _sut.GetAll();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Update_WhenInvalid_ReturnsBadRequest()
    {
        // Arrange
        var vr = new ValidationResult();
        vr.Errors.Add("x");
        _update.Setup(v => v.Validate(It.IsAny<UpdateItemEstoqueRequest>())).Returns(vr);

        // Act
        var result = await _sut.Update(1, EmptyUpdate());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_WhenValid_ReturnsOk()
    {
        // Arrange
        _update.Setup(v => v.Validate(It.IsAny<UpdateItemEstoqueRequest>())).Returns(new ValidationResult());
        _mediator.Setup(m => m.Send(It.IsAny<UpdateItemEstoqueCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Resp());

        // Act
        var result = await _sut.Update(1, new UpdateItemEstoqueRequest
        {
            Codigo = "A",
            Nome = "N",
            Tipo = ItemTipo.Peca,
            UnidadeMedida = UnidadeMedida.Unidade,
            PrecoVenda = 1
        });

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<DeleteItemEstoqueCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RegistrarEntrada_WhenInvalid_ReturnsBadRequest()
    {
        // Arrange
        var vr = new ValidationResult();
        vr.Errors.Add("x");
        _entrada.Setup(v => v.Validate(It.IsAny<RegistrarEntradaEstoqueRequest>())).Returns(vr);

        // Act
        var result = await _sut.RegistrarEntrada(1, new RegistrarEntradaEstoqueRequest { Quantidade = 0 });

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RegistrarEntrada_WhenValid_ReturnsOk()
    {
        // Arrange
        _entrada.Setup(v => v.Validate(It.IsAny<RegistrarEntradaEstoqueRequest>())).Returns(new ValidationResult());
        _mediator.Setup(m => m.Send(It.IsAny<RegistrarEntradaEstoqueCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Resp());

        // Act
        var result = await _sut.RegistrarEntrada(1, new RegistrarEntradaEstoqueRequest { Quantidade = 2 });

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
