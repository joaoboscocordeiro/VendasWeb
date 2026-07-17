using FrenteCaixa.Estoque.Application.Estoques.Interfaces;
using FrenteCaixa.Estoque.Application.Estoques.Repositorios;
using FrenteCaixa.Estoque.Domain.Estoques;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FrenteCaixa.Estoque.Tests;

public sealed class EstoqueApiFactory : WebApplicationFactory<Program>
{
    public const string JwtIssuer = "FrenteCaixa.Identidade.Testes";
    public const string JwtAudience = "FrenteCaixa.Backend.Testes";
    public const string JwtKey = "chave-de-testes-com-mais-de-32-bytes-para-estoque";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Estoque"] = "Host=localhost;Database=frente_caixa_estoque_testes",
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:Chave"] = JwtKey,
                ["RabbitMqConsumers:ProdutoCriado:Habilitado"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEstoqueRepositorio>();
            services.RemoveAll<IUnidadeTrabalho>();
            services.RemoveAll<IRelogio>();
            services.RemoveAll<IInboxRepositorioEstoque>();

            services.AddSingleton<BancoEstoqueEmMemoria>();
            services.AddScoped<IEstoqueRepositorio, EstoqueRepositorioEmMemoria>();
            services.AddScoped<IInboxRepositorioEstoque, InboxRepositorioEstoqueEmMemoria>();
            services.AddScoped<IUnidadeTrabalho, UnidadeTrabalhoEmMemoria>();
            services.AddSingleton<IRelogio, RelogioFixo>();
        });
    }

    public async Task<SaldoProduto> SemearSaldoAsync(Guid produtoId, decimal quantidade)
    {
        using var scope = Services.CreateScope();
        var banco = scope.ServiceProvider.GetRequiredService<BancoEstoqueEmMemoria>();
        var saldo = SaldoProduto.Criar(produtoId, DateTimeOffset.UtcNow);

        if (quantidade > 0)
        {
            var movimentacao = saldo.AplicarAjuste(
                TipoMovimentacaoEstoque.Entrada,
                quantidade,
                "Carga inicial de teste",
                DateTimeOffset.UtcNow);

            banco.Movimentacoes.Add(movimentacao);
        }

        banco.Saldos.Add(saldo);
        await Task.CompletedTask;

        return saldo;
    }

    public void Limpar()
    {
        using var scope = Services.CreateScope();
        var banco = scope.ServiceProvider.GetRequiredService<BancoEstoqueEmMemoria>();
        banco.Limpar();
    }
}
