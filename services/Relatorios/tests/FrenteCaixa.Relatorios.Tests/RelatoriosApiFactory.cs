using FrenteCaixa.Relatorios.Application.Relatorios.Interfaces;
using FrenteCaixa.Relatorios.Application.Relatorios.Repositorios;
using FrenteCaixa.Relatorios.Domain.Relatorios;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FrenteCaixa.Relatorios.Tests;

public sealed class RelatoriosApiFactory : WebApplicationFactory<Program>
{
    public const string JwtIssuer = "FrenteCaixa.Identidade.Testes";
    public const string JwtAudience = "FrenteCaixa.Backend.Testes";
    public const string JwtKey = "chave-de-testes-com-mais-de-32-bytes-para-relatorios";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Relatorios"] = "Host=localhost;Database=frente_caixa_relatorios_testes",
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:Chave"] = JwtKey,
                ["RabbitMqConsumers:VendaConcluida:Habilitado"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IRelatoriosRepositorio>();
            services.RemoveAll<IInboxRepositorioRelatorios>();
            services.RemoveAll<IUnidadeTrabalho>();
            services.RemoveAll<IRelogio>();

            services.AddSingleton<BancoRelatoriosEmMemoria>();
            services.AddScoped<IRelatoriosRepositorio, RelatoriosRepositorioEmMemoria>();
            services.AddScoped<IInboxRepositorioRelatorios, InboxRepositorioRelatoriosEmMemoria>();
            services.AddScoped<IUnidadeTrabalho, UnidadeTrabalhoEmMemoria>();
            services.AddSingleton<IRelogio, RelogioFixo>();
        });
    }

    public async Task<VendaConcluidaProjetada> SemearVendaAsync(
        string formaPagamento = "Dinheiro",
        decimal valorTotal = 20m)
    {
        using var scope = Services.CreateScope();
        var banco = scope.ServiceProvider.GetRequiredService<BancoRelatoriosEmMemoria>();
        var venda = VendaConcluidaProjetada.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            formaPagamento,
            valorTotal,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        venda.AdicionarItem(Guid.NewGuid(), "Cafe Torrado 500g", 2m, 10m, valorTotal);
        banco.Vendas.Add(venda);
        await Task.CompletedTask;

        return venda;
    }

    public BancoRelatoriosEmMemoria ObterBanco()
    {
        using var scope = Services.CreateScope();

        return scope.ServiceProvider.GetRequiredService<BancoRelatoriosEmMemoria>();
    }
}
