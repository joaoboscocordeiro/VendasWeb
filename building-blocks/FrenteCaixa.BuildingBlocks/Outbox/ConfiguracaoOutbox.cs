using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.BuildingBlocks.Outbox;

public static class ConfiguracaoOutbox
{
    public static IServiceCollection AdicionarOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.Secao));
        services.AddSingleton<IRelogio, RelogioSistema>();
        services.AddScoped<ProcessadorOutbox>();
        services.AddHostedService<OutboxBackgroundService>();

        return services;
    }
}
