using FrenteCaixa.Caixa.Application.Caixas.Interfaces;
using FrenteCaixa.Caixa.Application.Caixas.Repositorios;
using FrenteCaixa.Caixa.Domain.Caixas;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FrenteCaixa.Caixa.Tests;

public sealed class CaixaApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid OperadorPadraoId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid OutroOperadorId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public const string JwtIssuer = "FrenteCaixa.Identidade.Testes";
    public const string JwtAudience = "FrenteCaixa.Backend.Testes";
    public const string JwtKey = "chave-de-testes-com-mais-de-32-bytes-para-caixa";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Caixa"] = "Host=localhost;Database=frente_caixa_caixa_testes",
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:Chave"] = JwtKey
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICaixaRepositorio>();
            services.RemoveAll<IUnidadeTrabalho>();
            services.RemoveAll<IRelogio>();

            services.AddSingleton<BancoCaixaEmMemoria>();
            services.AddScoped<ICaixaRepositorio, CaixaRepositorioEmMemoria>();
            services.AddScoped<IUnidadeTrabalho, UnidadeTrabalhoEmMemoria>();
            services.AddSingleton<IRelogio, RelogioFixo>();
        });
    }

    public async Task<CaixaOperacional> SemearCaixaAbertoAsync(
        Guid? operadorId = null,
        decimal valorInicial = 100m)
    {
        using var scope = Services.CreateScope();
        var banco = scope.ServiceProvider.GetRequiredService<BancoCaixaEmMemoria>();
        var caixa = CaixaOperacional.Abrir(
            operadorId ?? OperadorPadraoId,
            valorInicial,
            DateTimeOffset.UtcNow);

        banco.Caixas.Add(caixa);
        banco.Movimentacoes.Add(caixa.RegistrarAbertura(DateTimeOffset.UtcNow));
        await Task.CompletedTask;

        return caixa;
    }

    public void Limpar()
    {
        using var scope = Services.CreateScope();
        var banco = scope.ServiceProvider.GetRequiredService<BancoCaixaEmMemoria>();
        banco.Limpar();
    }
}
