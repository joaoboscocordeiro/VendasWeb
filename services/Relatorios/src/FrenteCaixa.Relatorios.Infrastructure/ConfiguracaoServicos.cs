using FrenteCaixa.BuildingBlocks.Mensageria;
using FrenteCaixa.Relatorios.Application.Relatorios;
using FrenteCaixa.Relatorios.Application.Relatorios.Eventos;
using FrenteCaixa.Relatorios.Application.Relatorios.Interfaces;
using FrenteCaixa.Relatorios.Application.Relatorios.Repositorios;
using FrenteCaixa.Relatorios.Infrastructure.Mensageria;
using FrenteCaixa.Relatorios.Infrastructure.Persistencia;
using FrenteCaixa.Relatorios.Infrastructure.Persistencia.Repositorios;
using FrenteCaixa.Relatorios.Infrastructure.Tempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.Relatorios.Infrastructure;

public static class ConfiguracaoServicos
{
    public static IServiceCollection AdicionarInfraestruturaRelatorios(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Relatorios");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A connection string ConnectionStrings:Relatorios e obrigatoria.");
        }

        services.AddDbContext<RelatoriosDbContext>(options => options.UseNpgsql(connectionString));
        services.AdicionarRabbitMq(configuration);
        services.Configure<VendaConcluidaConsumerOptions>(
            configuration.GetSection(VendaConcluidaConsumerOptions.Secao));
        services.AddScoped<IRelatoriosRepositorio, RelatoriosRepositorio>();
        services.AddScoped<IInboxRepositorioRelatorios, InboxRepositorioRelatorios>();
        services.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();
        services.AddScoped<IServicoRelatorios, ServicoRelatorios>();
        services.AddScoped<IProcessadorVendaConcluida, ProcessadorVendaConcluida>();
        services.AddSingleton<IRelogio, RelogioSistema>();
        services.AddHostedService<VendaConcluidaConsumerBackgroundService>();

        return services;
    }
}
