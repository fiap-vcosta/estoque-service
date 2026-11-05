using Application.Abstractions.Gateways;
using Application.Abstractions.Persistence;
using Application.Messaging;
using Application.Services;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using BuildingBlocks.Tracing;
using Infrastructure.Database;
using Infrastructure.Database.Gateways;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public const string SubscriptionOrdemServicoAprovada = "estoque-ordem-servico-aprovada";
    public const string SubscriptionOrdemServicoFinalizada = "estoque-ordem-servico-finalizada";
    public const string SubscriptionOrdemServicoDescartada = "estoque-ordem-servico-descartada";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IItemEstoqueGateway, ItemEstoqueGateway>();
        services.AddScoped<IReservaEstoqueGateway, ReservaEstoqueGateway>();
        services.AddScoped<IEventoProcessadoGateway, EventoProcessadoGateway>();
        services.AddScoped<IOrdemDescartadaGateway, OrdemDescartadaGateway>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ReservaEstoqueService>();

        var useInMemoryPubSub = configuration.GetValue("PubSub:UseInMemory", false);
        if (useInMemoryPubSub)
        {
            services.AddSingleton<InMemoryPubSubTransport>();
            services.AddSingleton<IPubSubTransport>(sp => sp.GetRequiredService<InMemoryPubSubTransport>());
            services.AddSingleton<IMessageTracePropagator, DatadogMessageTracePropagator>();
            services.AddSingleton<IEventPublisher, EventPublisher>();
            services.AddOptions<PubSubOptions>();
        }
        else
        {
            services.Configure<PubSubOptions>(configuration.GetSection(PubSubOptions.SectionName));
            services.AddBuildingBlocksMessaging();
        }

        if (configuration.GetValue("PubSub:EnableWorkers", true))
        {
            services.AddEventSubscriptionWorker<OrdemServicoAprovadaData, OrdemServicoAprovadaConsumer>(
                configuration["PubSub:Subscriptions:OrdemServicoAprovada"] ?? SubscriptionOrdemServicoAprovada);
            services.AddEventSubscriptionWorker<OrdemServicoFinalizadaData, OrdemServicoFinalizadaConsumer>(
                configuration["PubSub:Subscriptions:OrdemServicoFinalizada"] ?? SubscriptionOrdemServicoFinalizada);
            services.AddEventSubscriptionWorker<OrdemServicoDescartadaData, OrdemServicoDescartadaConsumer>(
                configuration["PubSub:Subscriptions:OrdemServicoDescartada"] ?? SubscriptionOrdemServicoDescartada);
        }

        return services;
    }
}
