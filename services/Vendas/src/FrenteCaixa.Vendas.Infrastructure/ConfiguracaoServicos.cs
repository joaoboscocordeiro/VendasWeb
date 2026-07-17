using FrenteCaixa.BuildingBlocks.Mensageria;
using FrenteCaixa.BuildingBlocks.Outbox;
using FrenteCaixa.Vendas.Application.Vendas;
using FrenteCaixa.Vendas.Application.Vendas.Eventos;
using FrenteCaixa.Vendas.Application.Vendas.Interfaces;
using FrenteCaixa.Vendas.Application.Vendas.Integracoes;
using FrenteCaixa.Vendas.Application.Vendas.Repositorios;
using FrenteCaixa.Vendas.Infrastructure.Integracoes;
using FrenteCaixa.Vendas.Infrastructure.Mensageria;
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
        services.AdicionarRabbitMq(configuration);
        services.AdicionarOutbox(configuration);
        services.AddScoped<IVendaRepositorio, VendaRepositorio>();
        services.AddScoped<IRepositorioOutbox, OutboxRepositorio>();
        services.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();
        services.AddScoped<IRegistradorEventosVenda, RegistradorEventosVendaOutbox>();
        services.AddScoped<IServicoVendas, ServicoVendas>();
        services.AddSingleton<
            FrenteCaixa.Vendas.Application.Vendas.Interfaces.IRelogio,
            FrenteCaixa.Vendas.Infrastructure.Tempo.RelogioSistema>();
        services.AddHttpClient<IClienteCaixa, CaixaHttpClient>(client =>
        {
            client.BaseAddress = ObterBaseUrl(configuration, "ServicosExternos:CaixaBaseUrl", new Uri("http://localhost:5062"));
        });
        services.AddHttpClient<IClienteEstoque, EstoqueHttpClient>(client =>
        {
            client.BaseAddress = ObterBaseUrl(configuration, "ServicosExternos:EstoqueBaseUrl", new Uri("http://localhost:5252"));
        });
        services.AddHttpClient<IClientePagamentos, PagamentosHttpClient>(client =>
        {
            client.BaseAddress = ObterBaseUrl(configuration, "ServicosExternos:PagamentosBaseUrl", new Uri("http://localhost:5216"));
        });

        return services;
    }

    private static Uri ObterBaseUrl(IConfiguration configuration, string chave, Uri valorPadrao)
    {
        var valor = configuration[chave];

        return Uri.TryCreate(valor, UriKind.Absolute, out var uri) ? uri : valorPadrao;
    }
}
