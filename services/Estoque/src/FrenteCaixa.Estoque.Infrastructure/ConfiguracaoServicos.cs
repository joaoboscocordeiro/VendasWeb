using FrenteCaixa.BuildingBlocks.Mensageria;
using FrenteCaixa.Estoque.Application.Estoques;
using FrenteCaixa.Estoque.Application.Estoques.Eventos;
using FrenteCaixa.Estoque.Application.Estoques.Interfaces;
using FrenteCaixa.Estoque.Application.Estoques.Repositorios;
using FrenteCaixa.Estoque.Infrastructure.Mensageria;
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
        services.AdicionarRabbitMq(configuration);
        services.Configure<ProdutoCriadoConsumerOptions>(
            configuration.GetSection(ProdutoCriadoConsumerOptions.Secao));
        services.AddScoped<IEstoqueRepositorio, EstoqueRepositorio>();
        services.AddScoped<IInboxRepositorioEstoque, InboxRepositorioEstoque>();
        services.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();
        services.AddScoped<IServicoEstoque, ServicoEstoque>();
        services.AddScoped<IProcessadorProdutoCriado, ProcessadorProdutoCriado>();
        services.AddSingleton<IRelogio, RelogioSistema>();
        services.AddHostedService<ProdutoCriadoConsumerBackgroundService>();

        return services;
    }
}
