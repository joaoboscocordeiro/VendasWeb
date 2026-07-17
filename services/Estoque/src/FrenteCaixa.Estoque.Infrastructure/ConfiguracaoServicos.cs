using FrenteCaixa.Estoque.Application.Estoques;
using FrenteCaixa.Estoque.Application.Estoques.Interfaces;
using FrenteCaixa.Estoque.Application.Estoques.Repositorios;
using FrenteCaixa.Estoque.Infrastructure.Persistencia;
using FrenteCaixa.Estoque.Infrastructure.Persistencia.Repositorios;
using FrenteCaixa.Estoque.Infrastructure.Tempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.Estoque.Infrastructure;

public static class ConfiguracaoServicos
{
    public static IServiceCollection AdicionarInfraestruturaEstoque(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Estoque");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A connection string ConnectionStrings:Estoque e obrigatoria.");
        }

        services.AddDbContext<EstoqueDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IEstoqueRepositorio, EstoqueRepositorio>();
        services.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();
        services.AddScoped<IServicoEstoque, ServicoEstoque>();
        services.AddSingleton<IRelogio, RelogioSistema>();

        return services;
    }
}
