using FrenteCaixa.Identidade.Application.Autenticacao;
using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;
using FrenteCaixa.Identidade.Application.Autenticacao.Repositorios;
using FrenteCaixa.Identidade.Infrastructure.Persistencia;
using FrenteCaixa.Identidade.Infrastructure.Persistencia.Repositorios;
using FrenteCaixa.Identidade.Infrastructure.Seguranca;
using FrenteCaixa.Identidade.Infrastructure.Tempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.Identidade.Infrastructure;

public static class ConfiguracaoServicos
{
    public static IServiceCollection AdicionarInfraestruturaIdentidade(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Identidade");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A connection string ConnectionStrings:Identidade e obrigatoria.");
        }

        services.AddDbContext<IdentidadeDbContext>(options => options.UseNpgsql(connectionString));
        services.Configure<ConfiguracaoJwt>(opcoes =>
        {
            var secaoJwt = configuration.GetSection("Jwt");
            opcoes.Issuer = secaoJwt["Issuer"] ?? opcoes.Issuer;
            opcoes.Audience = secaoJwt["Audience"] ?? opcoes.Audience;
            opcoes.Chave = secaoJwt["Chave"] ?? opcoes.Chave;

            if (int.TryParse(secaoJwt["AccessTokenMinutos"], out var accessTokenMinutos))
            {
                opcoes.AccessTokenMinutos = accessTokenMinutos;
            }
        });

        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.AddScoped<IRefreshTokenRepositorio, RefreshTokenRepositorio>();
        services.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();
        services.AddScoped<IServicoAutenticacao, ServicoAutenticacao>();
        services.AddSingleton<ISenhaHasher, SenhaHasher>();
        services.AddSingleton<IRefreshTokenHasher, RefreshTokenHasher>();
        services.AddSingleton<IGeradorRefreshToken, GeradorRefreshToken>();
        services.AddSingleton<IRelogio, RelogioSistema>();
        services.AddScoped<IGeradorJwt, GeradorJwt>();

        return services;
    }
}
