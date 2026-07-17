using FrenteCaixa.Pagamentos.Application.Pagamentos.Interfaces;
using FrenteCaixa.Pagamentos.Application.Pagamentos.Repositorios;
using FrenteCaixa.Pagamentos.Domain.Pagamentos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FrenteCaixa.Pagamentos.Tests;

public sealed class PagamentosApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid VendaPadraoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid OutraVendaId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public const string JwtIssuer = "FrenteCaixa.Identidade.Testes";
    public const string JwtAudience = "FrenteCaixa.Backend.Testes";
    public const string JwtKey = "chave-de-testes-com-mais-de-32-bytes-para-pagamentos";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Pagamentos"] = "Host=localhost;Database=frente_caixa_pagamentos_testes",
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:Chave"] = JwtKey
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPagamentoRepositorio>();
            services.RemoveAll<IUnidadeTrabalho>();
            services.RemoveAll<IRelogio>();

            services.AddSingleton<BancoPagamentosEmMemoria>();
            services.AddScoped<IPagamentoRepositorio, PagamentoRepositorioEmMemoria>();
            services.AddScoped<IUnidadeTrabalho, UnidadeTrabalhoEmMemoria>();
            services.AddSingleton<IRelogio, RelogioFixo>();
        });
    }

    public async Task<Pagamento> SemearPagamentoAsync(Guid? vendaId = null)
    {
        using var scope = Services.CreateScope();
        var banco = scope.ServiceProvider.GetRequiredService<BancoPagamentosEmMemoria>();
        var pagamento = Pagamento.Registrar(
            vendaId ?? VendaPadraoId,
            FormaPagamento.Pix,
            37.50m,
            37.50m,
            DateTimeOffset.UtcNow);

        banco.Pagamentos.Add(pagamento);
        await Task.CompletedTask;

        return pagamento;
    }
}
