using FrenteCaixa.BuildingBlocks.Mensageria;
using FrenteCaixa.Caixa.Application.Caixas;
using FrenteCaixa.Caixa.Application.Caixas.Eventos;
using FrenteCaixa.Caixa.Application.Caixas.Interfaces;
using FrenteCaixa.Caixa.Application.Caixas.Repositorios;
using FrenteCaixa.Caixa.Infrastructure.Mensageria;
using FrenteCaixa.Caixa.Infrastructure.Persistencia;
using FrenteCaixa.Caixa.Infrastructure.Persistencia.Repositorios;
using FrenteCaixa.Caixa.Infrastructure.Tempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.Caixa.Infrastructure;

public static class ConfiguracaoServicos
{
    public static IServiceCollection AdicionarInfraestruturaCaixa(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Caixa");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A connection string ConnectionStrings:Caixa e obrigatoria.");
        }

        services.AddDbContext<CaixaDbContext>(options => options.UseNpgsql(connectionString));
        services.AdicionarRabbitMq(configuration);
        services.Configure<VendaConcluidaConsumerOptions>(
            configuration.GetSection(VendaConcluidaConsumerOptions.Secao));
        services.AddScoped<ICaixaRepositorio, CaixaRepositorio>();
        services.AddScoped<IInboxRepositorioCaixa, InboxRepositorioCaixa>();
        services.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();
        services.AddScoped<IServicoCaixa, ServicoCaixa>();
        services.AddScoped<IProcessadorVendaConcluidaCaixa, ProcessadorVendaConcluidaCaixa>();
        services.AddSingleton<IRelogio, RelogioSistema>();
        services.AddHostedService<VendaConcluidaConsumerBackgroundService>();

        return services;
    }
}
