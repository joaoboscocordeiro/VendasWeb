using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;
using FrenteCaixa.Identidade.Application.Autenticacao.Repositorios;
using FrenteCaixa.Identidade.Domain.Usuarios;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FrenteCaixa.Identidade.Tests;

public sealed class IdentidadeApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Identidade"] = "Host=localhost;Database=frente_caixa_identidade_testes",
                ["Jwt:Issuer"] = "FrenteCaixa.Identidade.Testes",
                ["Jwt:Audience"] = "FrenteCaixa.Backend.Testes",
                ["Jwt:Chave"] = "chave-de-testes-com-mais-de-32-bytes-para-jwt",
                ["Jwt:AccessTokenMinutos"] = "15"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUsuarioRepositorio>();
            services.RemoveAll<IRefreshTokenRepositorio>();
            services.RemoveAll<IUnidadeTrabalho>();

            services.AddSingleton<BancoIdentidadeEmMemoria>();
            services.AddScoped<IUsuarioRepositorio, UsuarioRepositorioEmMemoria>();
            services.AddScoped<IRefreshTokenRepositorio, RefreshTokenRepositorioEmMemoria>();
            services.AddScoped<IUnidadeTrabalho, UnidadeTrabalhoEmMemoria>();
        });
    }

    public async Task SemearUsuariosAsync()
    {
        using var scope = Services.CreateScope();
        var banco = scope.ServiceProvider.GetRequiredService<BancoIdentidadeEmMemoria>();
        var senhaHasher = scope.ServiceProvider.GetRequiredService<ISenhaHasher>();
        var agora = DateTimeOffset.UtcNow;

        var administrador = Usuario.Criar(
            "Administrador",
            "admin@frentecaixa.local",
            senhaHasher.GerarHash("Senha@123"),
            PerfilUsuario.ADM,
            agora);

        var vendedor = Usuario.Criar(
            "Vendedor",
            "vendedor@frentecaixa.local",
            senhaHasher.GerarHash("Senha@123"),
            PerfilUsuario.VENDEDOR,
            agora);

        var inativo = Usuario.Criar(
            "Vendedor Inativo",
            "inativo@frentecaixa.local",
            senhaHasher.GerarHash("Senha@123"),
            PerfilUsuario.VENDEDOR,
            agora);

        inativo.Inativar(agora);

        banco.Limpar();
        banco.Usuarios.AddRange(new[] { administrador, vendedor, inativo });
        await Task.CompletedTask;
    }
}
