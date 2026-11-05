using System.Text.Json.Serialization;
using Api.Contracts.Validation;
using Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;
using Api.Controllers.Staff.ItemEstoque.RegistrarEntradaEstoque;
using Api.Controllers.Staff.ItemEstoque.UpdateItemEstoque;
using Api.Presenters.ItemEstoque;
using BuildingBlocks.Http.Auth;
using BuildingBlocks.Http.ProblemDetails;
using Domain.Exceptions;
using Infrastructure.DependencyInjection;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddScoped<ItemEstoquePresenter>();

        services.AddSingleton<IValidator<CreateItemEstoqueRequest>, CreateItemEstoqueRequestValidator>();
        services.AddSingleton<IValidator<UpdateItemEstoqueRequest>, UpdateItemEstoqueRequestValidator>();
        services.AddSingleton<IValidator<RegistrarEntradaEstoqueRequest>, RegistrarEntradaEstoqueRequestValidator>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Application.UseCases.ItemEstoque.Commands.CreateItemEstoque.CreateItemEstoqueCommand).Assembly));

        services.AddBuildingBlocksProblemDetails(options =>
        {
            options.Map<DomainNotFoundException>(StatusCodes.Status404NotFound, "Not Found");
            options.Map<KeyNotFoundException>(StatusCodes.Status404NotFound, "Not Found");
            options.Map<BusinessRuleException>(StatusCodes.Status400BadRequest, "Business Rule Violation");
            options.Map<ArgumentException>(StatusCodes.Status400BadRequest, "Bad Request");
            options.Map<UnauthorizedAccessException>(StatusCodes.Status401Unauthorized, "Unauthorized");
        });

        services.AddControllers(options =>
            {
                options.Filters.Add<ProblemDetailsExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddBuildingBlocksJwtAuthentication(configuration);
    }
}
