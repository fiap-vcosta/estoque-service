using Api.Contracts.Validation;
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
using BuildingBlocks.Http.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Staff.ItemEstoque;

[ApiController]
[Route("estoque/itens")]
[Authorize(Policy = AuthPolicies.FuncionarioJwt)]
public class ItemEstoqueController(
    IMediator mediator,
    ItemEstoquePresenter presenter,
    IValidator<CreateItemEstoqueRequest> createValidator,
    IValidator<UpdateItemEstoqueRequest> updateValidator,
    IValidator<RegistrarEntradaEstoqueRequest> registrarEntradaValidator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemEstoqueRequest request)
    {
        var validationResult = createValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { validationResult.Errors });
        }

        var response = await mediator.Send(new CreateItemEstoqueCommand
        {
            Codigo = request.Codigo,
            Tipo = request.Tipo,
            Nome = request.Nome,
            UnidadeMedida = request.UnidadeMedida,
            PrecoVenda = request.PrecoVenda,
            Saldo = request.Saldo,
            SaldoReservado = request.SaldoReservado
        });

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, presenter.Present(response));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await mediator.Send(new GetItemEstoqueByIdQuery { Id = id });
        return response is null ? NotFound() : Ok(presenter.Present(response));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await mediator.Send(new GetAllItensEstoqueQuery());
        return Ok(presenter.Present(response));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateItemEstoqueRequest request)
    {
        var validationResult = updateValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { validationResult.Errors });
        }

        var response = await mediator.Send(new UpdateItemEstoqueCommand
        {
            Id = id,
            Codigo = request.Codigo,
            Tipo = request.Tipo,
            Nome = request.Nome,
            UnidadeMedida = request.UnidadeMedida,
            PrecoVenda = request.PrecoVenda
        });

        return Ok(presenter.Present(response));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteItemEstoqueCommand { Id = id });
        return NoContent();
    }

    [HttpPost("{id:int}/registrar-entrada")]
    public async Task<IActionResult> RegistrarEntrada(int id, [FromBody] RegistrarEntradaEstoqueRequest request)
    {
        var validationResult = registrarEntradaValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { validationResult.Errors });
        }

        var response = await mediator.Send(new RegistrarEntradaEstoqueCommand
        {
            IdItemEstoque = id,
            QuantidadeRecebida = request.Quantidade
        });

        return Ok(presenter.Present(response));
    }
}
