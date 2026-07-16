using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.BuildingBlocks.Mensageria;

public static class ConfiguracaoMensageria
{
    public static IServiceCollection AdicionarRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.Secao));
        services.AddSingleton<IPublicadorEventos, RabbitMqPublicadorEventos>();

        return services;
    }
}
