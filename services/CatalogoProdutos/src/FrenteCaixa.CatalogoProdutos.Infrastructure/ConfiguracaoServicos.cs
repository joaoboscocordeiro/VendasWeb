using FrenteCaixa.CatalogoProdutos.Application.Produtos;
using FrenteCaixa.CatalogoProdutos.Application.Produtos.Interfaces;
using FrenteCaixa.CatalogoProdutos.Application.Produtos.Repositorios;
using FrenteCaixa.CatalogoProdutos.Infrastructure.Persistencia;
using FrenteCaixa.CatalogoProdutos.Infrastructure.Persistencia.Repositorios;
using FrenteCaixa.CatalogoProdutos.Infrastructure.Tempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.CatalogoProdutos.Infrastructure;

public static class ConfiguracaoServicos
{
    public static IServiceCollection AdicionarInfraestruturaCatalogoProdutos(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CatalogoProdutos");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A connection string ConnectionStrings:CatalogoProdutos e obrigatoria.");
        }

        services.AddDbContext<CatalogoProdutosDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IProdutoRepositorio, ProdutoRepositorio>();
        services.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();
        services.AddScoped<IServicoProdutos, ServicoProdutos>();
        services.AddSingleton<IRelogio, RelogioSistema>();

        return services;
    }
}
