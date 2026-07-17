using FrenteCaixa.Vendas.Application.Vendas;
using FrenteCaixa.Vendas.Application.Vendas.Interfaces;
using FrenteCaixa.Vendas.Application.Vendas.Repositorios;
using FrenteCaixa.Vendas.Infrastructure.Persistencia;
using FrenteCaixa.Vendas.Infrastructure.Persistencia.Repositorios;
using FrenteCaixa.Vendas.Infrastructure.Tempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.Vendas.Infrastructure;

public static class ConfiguracaoServicos
{
    public static IServiceCollection AdicionarInfraestruturaVendas(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Vendas");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A connection string ConnectionStrings:Vendas e obrigatoria.");
        }

        services.AddDbContext<VendasDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IVendaRepositorio, VendaRepositorio>();
        services.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();
        services.AddScoped<IServicoVendas, ServicoVendas>();
        services.AddSingleton<IRelogio, RelogioSistema>();

        return services;
    }
}
