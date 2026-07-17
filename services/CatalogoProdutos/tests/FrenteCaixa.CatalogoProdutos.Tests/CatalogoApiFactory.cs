using FrenteCaixa.CatalogoProdutos.Application.Produtos.Eventos;
using FrenteCaixa.CatalogoProdutos.Application.Produtos.Interfaces;
using FrenteCaixa.CatalogoProdutos.Application.Produtos.Repositorios;
using FrenteCaixa.CatalogoProdutos.Domain.Produtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FrenteCaixa.CatalogoProdutos.Tests;

public sealed class CatalogoApiFactory : WebApplicationFactory<Program>
{
    public const string JwtIssuer = "FrenteCaixa.Identidade.Testes";
    public const string JwtAudience = "FrenteCaixa.Backend.Testes";
    public const string JwtKey = "chave-de-testes-com-mais-de-32-bytes-para-catalogo";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CatalogoProdutos"] = "Host=localhost;Database=frente_caixa_catalogo_testes",
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:Chave"] = JwtKey,
                ["Outbox:Habilitado"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IProdutoRepositorio>();
            services.RemoveAll<IUnidadeTrabalho>();
            services.RemoveAll<IRelogio>();
            services.RemoveAll<IRegistradorEventosProduto>();

            services.AddSingleton<BancoCatalogoEmMemoria>();
            services.AddScoped<IProdutoRepositorio, ProdutoRepositorioEmMemoria>();
            services.AddScoped<IUnidadeTrabalho, UnidadeTrabalhoEmMemoria>();
            services.AddSingleton<IRelogio, RelogioFixo>();
            services.AddScoped<IRegistradorEventosProduto, RegistradorEventosProdutoEmMemoria>();
        });
    }

    public async Task<Produto> SemearProdutoAsync(
        string descricao = "Cafe Torrado 500g",
        string? codigoBarras = "7891234567895",
        decimal precoCusto = 8.50m,
        decimal precoVenda = 14.90m)
    {
        using var scope = Services.CreateScope();
        var banco = scope.ServiceProvider.GetRequiredService<BancoCatalogoEmMemoria>();
        var produto = Produto.Criar(
            descricao,
            codigoBarras,
            precoCusto,
            precoVenda,
            DateTimeOffset.UtcNow);

        banco.Produtos.Add(produto);
        await Task.CompletedTask;

        return produto;
    }

    public void Limpar()
    {
        using var scope = Services.CreateScope();
        var banco = scope.ServiceProvider.GetRequiredService<BancoCatalogoEmMemoria>();
        banco.Limpar();
    }
}
