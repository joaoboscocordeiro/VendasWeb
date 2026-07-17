using FrenteCaixa.Vendas.Application.Vendas.Eventos;
using FrenteCaixa.Vendas.Application.Vendas.Interfaces;
using FrenteCaixa.Vendas.Application.Vendas.Integracoes;
using FrenteCaixa.Vendas.Application.Vendas.Repositorios;
using FrenteCaixa.Vendas.Domain.Vendas;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FrenteCaixa.Vendas.Tests;

public sealed class VendasApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid OperadorPadraoId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid OutroOperadorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CaixaPadraoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public const string JwtIssuer = "FrenteCaixa.Identidade.Testes";
    public const string JwtAudience = "FrenteCaixa.Backend.Testes";
    public const string JwtKey = "chave-de-testes-com-mais-de-32-bytes-para-vendas";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Vendas"] = "Host=localhost;Database=frente_caixa_vendas_testes",
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:Chave"] = JwtKey,
                ["Outbox:Habilitado"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IVendaRepositorio>();
            services.RemoveAll<IUnidadeTrabalho>();
            services.RemoveAll<IRelogio>();
            services.RemoveAll<IClienteCaixa>();
            services.RemoveAll<IClienteEstoque>();
            services.RemoveAll<IClientePagamentos>();
            services.RemoveAll<IRegistradorEventosVenda>();

            services.AddSingleton<BancoVendasEmMemoria>();
            services.AddScoped<IVendaRepositorio, VendaRepositorioEmMemoria>();
            services.AddScoped<IUnidadeTrabalho, UnidadeTrabalhoEmMemoria>();
            services.AddSingleton<IRelogio, RelogioFixo>();
            services.AddScoped<IClienteCaixa, ClienteCaixaEmMemoria>();
            services.AddScoped<IClienteEstoque, ClienteEstoqueEmMemoria>();
            services.AddScoped<IClientePagamentos, ClientePagamentosEmMemoria>();
            services.AddScoped<IRegistradorEventosVenda, RegistradorEventosVendaEmMemoria>();
        });
    }

    public async Task<Venda> SemearVendaAsync(Guid? operadorId = null)
    {
        using var scope = Services.CreateScope();
        var banco = scope.ServiceProvider.GetRequiredService<BancoVendasEmMemoria>();
        var venda = Venda.Iniciar(
            operadorId ?? OperadorPadraoId,
            CaixaPadraoId,
            DateTimeOffset.UtcNow);

        banco.Vendas.Add(venda);
        await Task.CompletedTask;

        return venda;
    }

    public async Task<Venda> SemearVendaComItensAsync()
    {
        var venda = await SemearVendaAsync();

        venda.AdicionarItem(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Cafe Torrado 500g",
            2,
            7.50m,
            DateTimeOffset.UtcNow);
        venda.AdicionarItem(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Acucar Cristal 1kg",
            1,
            5.90m,
            DateTimeOffset.UtcNow.AddSeconds(1));

        return venda;
    }

    public BancoVendasEmMemoria ObterBanco()
    {
        using var scope = Services.CreateScope();

        return scope.ServiceProvider.GetRequiredService<BancoVendasEmMemoria>();
    }
}
