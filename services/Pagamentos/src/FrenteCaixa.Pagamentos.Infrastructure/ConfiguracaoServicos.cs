using FrenteCaixa.Pagamentos.Application.Pagamentos;
using FrenteCaixa.Pagamentos.Application.Pagamentos.Interfaces;
using FrenteCaixa.Pagamentos.Application.Pagamentos.Repositorios;
using FrenteCaixa.Pagamentos.Infrastructure.Persistencia;
using FrenteCaixa.Pagamentos.Infrastructure.Persistencia.Repositorios;
using FrenteCaixa.Pagamentos.Infrastructure.Tempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.Pagamentos.Infrastructure;

public static class ConfiguracaoServicos
{
    public static IServiceCollection AdicionarInfraestruturaPagamentos(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Pagamentos");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A connection string ConnectionStrings:Pagamentos e obrigatoria.");
        }

        services.AddDbContext<PagamentosDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IPagamentoRepositorio, PagamentoRepositorio>();
        services.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();
        services.AddScoped<IServicoPagamentos, ServicoPagamentos>();
        services.AddSingleton<IRelogio, RelogioSistema>();

        return services;
    }
}
