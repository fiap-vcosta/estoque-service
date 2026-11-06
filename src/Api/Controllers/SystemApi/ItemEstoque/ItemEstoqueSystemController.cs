using Application.UseCases.ItemEstoque.Queries.GetItemEstoqueById;
using BuildingBlocks.Http.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.SystemApi.ItemEstoque;

[ApiController]
[AllowAnonymous]
[ServiceKeyAuthorize]
[Route("system/estoque/itens")]
public class ItemEstoqueSystemController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetItemEstoqueByIdQuery { Id = id }, cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = response.Id,
            codigo = response.Codigo,
            nome = response.Nome,
            unidadeMedida = response.UnidadeMedida.ToString()
        });
    }
}
