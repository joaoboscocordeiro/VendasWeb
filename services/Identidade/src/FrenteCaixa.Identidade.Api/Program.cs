using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FrenteCaixa.Identidade.Application.Autenticacao.Contratos;
using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;
using FrenteCaixa.Identidade.Application.Usuarios.Contratos;
using FrenteCaixa.Identidade.Application.Usuarios.Interfaces;
using FrenteCaixa.Identidade.Infrastructure;
using FrenteCaixa.Identidade.Infrastructure.Seguranca;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

GarantirConfiguracaoJwt(builder);

builder.Services.AddOpenApi();
builder.Services.AdicionarInfraestruturaIdentidade(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var configuracaoJwt = ObterConfiguracaoJwt(builder.Configuration);

        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuracaoJwt.Issuer,
            ValidateAudience = true,
            ValidAudience = configuracaoJwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracaoJwt.Chave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UsuarioAutenticado", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("SomenteAdministrador", policy => policy.RequireRole("ADM"));
    options.AddPolicy("SomenteVendedor", policy => policy.RequireRole("VENDEDOR"));
    options.AddPolicy("OperadorCaixa", policy => policy.RequireRole("ADM", "VENDEDOR"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var nomeServico = "Identidade";

app.MapGet("/api/saude", () =>
    Results.Ok(new RespostaSaude(nomeServico, "Operacional")))
    .WithName("ObterSaude");

app.MapGet("/api/info", () =>
    Results.Ok(new RespostaServico(nomeServico, "Autenticacao, usuarios, perfis e JWT")))
    .WithName("ObterInformacoesServico");

app.MapPost("/auth/login", async (
        LoginRequest request,
        IServicoAutenticacao servicoAutenticacao,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoAutenticacao.EntrarAsync(
            request,
            ObterEnderecoIp(httpContext),
            cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : Results.Unauthorized();
    })
    .WithName("Entrar");

app.MapPost("/auth/refresh", async (
        RenovarTokenRequest request,
        IServicoAutenticacao servicoAutenticacao,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoAutenticacao.RenovarAsync(
            request,
            ObterEnderecoIp(httpContext),
            cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : Results.Unauthorized();
    })
    .WithName("RenovarToken");

app.MapPost("/auth/logout", async (
        LogoutRequest request,
        IServicoAutenticacao servicoAutenticacao,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoAutenticacao.SairAsync(
            request,
            ObterEnderecoIp(httpContext),
            cancellationToken);

        return resultado.Sucesso
            ? Results.NoContent()
            : Results.Unauthorized();
    })
    .RequireAuthorization("UsuarioAutenticado")
    .WithName("Sair");

app.MapGet("/auth/me", async (
        ClaimsPrincipal usuarioLogado,
        IServicoAutenticacao servicoAutenticacao,
        CancellationToken cancellationToken) =>
    {
        var sub = usuarioLogado.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(sub, out var usuarioId))
        {
            return Results.Unauthorized();
        }

        var resultado = await servicoAutenticacao.ObterUsuarioAsync(usuarioId, cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : Results.Unauthorized();
    })
    .RequireAuthorization("UsuarioAutenticado")
    .WithName("ObterUsuarioAutenticado");

var usuarios = app.MapGroup("/users")
    .RequireAuthorization("SomenteAdministrador")
    .WithTags("Usuarios");

usuarios.MapPost("", async (
        CadastrarUsuarioRequest request,
        IServicoUsuarios servicoUsuarios,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoUsuarios.CadastrarAsync(request, cancellationToken);

        return resultado.Sucesso
            ? Results.Created($"/users/{resultado.Valor!.Id}", resultado.Valor)
            : MapearFalha(resultado);
    })
    .WithName("CadastrarUsuario");

usuarios.MapGet("", async (
        IServicoUsuarios servicoUsuarios,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoUsuarios.ListarAsync(cancellationToken);

        return Results.Ok(resultado);
    })
    .WithName("ListarUsuarios");

usuarios.MapGet("/{id:guid}", async (
        Guid id,
        IServicoUsuarios servicoUsuarios,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoUsuarios.ObterPorIdAsync(id, cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : MapearFalha(resultado);
    })
    .WithName("ObterUsuarioPorId");

usuarios.MapPut("/{id:guid}", async (
        Guid id,
        AtualizarUsuarioRequest request,
        IServicoUsuarios servicoUsuarios,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoUsuarios.AtualizarAsync(id, request, cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : MapearFalha(resultado);
    })
    .WithName("AtualizarUsuario");

usuarios.MapPatch("/{id:guid}/disable", async (
        Guid id,
        IServicoUsuarios servicoUsuarios,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoUsuarios.InativarAsync(id, cancellationToken);

        return resultado.Sucesso
            ? Results.NoContent()
            : MapearFalha(resultado);
    })
    .WithName("InativarUsuario");

app.MapGet("/administracao/verificacao", () => Results.Ok(new { Status = "Acesso administrativo autorizado" }))
    .RequireAuthorization("SomenteAdministrador")
    .WithName("VerificarAcessoAdministrativo");

app.Run();

static string? ObterEnderecoIp(HttpContext httpContext)
{
    return httpContext.Connection.RemoteIpAddress?.ToString();
}

static void GarantirConfiguracaoJwt(WebApplicationBuilder builder)
{
    var chave = builder.Configuration["Jwt:Chave"];

    if (string.IsNullOrWhiteSpace(chave))
    {
        if (builder.Environment.IsProduction())
        {
            throw new InvalidOperationException("Configure Jwt:Chave fora do codigo antes de iniciar em producao.");
        }

        chave = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        builder.Configuration["Jwt:Chave"] = chave;
    }

    if (Encoding.UTF8.GetByteCount(chave) < 32)
    {
        throw new InvalidOperationException("Jwt:Chave deve possuir pelo menos 32 bytes.");
    }
}

static ConfiguracaoJwt ObterConfiguracaoJwt(IConfiguration configuration)
{
    var secaoJwt = configuration.GetSection("Jwt");

    return new ConfiguracaoJwt
    {
        Issuer = secaoJwt["Issuer"] ?? "FrenteCaixa",
        Audience = secaoJwt["Audience"] ?? "FrenteCaixa.Backend",
        Chave = secaoJwt["Chave"] ?? string.Empty,
        AccessTokenMinutos = int.TryParse(secaoJwt["AccessTokenMinutos"], out var minutos) ? minutos : 15
    };
}

static IResult MapearFalha<T>(FrenteCaixa.Identidade.Application.Autenticacao.ResultadoOperacao<T> resultado)
{
    return resultado.CodigoErro switch
    {
        FrenteCaixa.Identidade.Application.Autenticacao.CodigoErroOperacao.Validacao =>
            Results.BadRequest(new RespostaErro(resultado.Erro ?? "Requisicao invalida.")),
        FrenteCaixa.Identidade.Application.Autenticacao.CodigoErroOperacao.Conflito =>
            Results.Conflict(new RespostaErro(resultado.Erro ?? "Conflito.")),
        FrenteCaixa.Identidade.Application.Autenticacao.CodigoErroOperacao.NaoEncontrado =>
            Results.NotFound(new RespostaErro(resultado.Erro ?? "Recurso nao encontrado.")),
        FrenteCaixa.Identidade.Application.Autenticacao.CodigoErroOperacao.NaoAutorizado =>
            Results.Unauthorized(),
        _ => Results.BadRequest(new RespostaErro(resultado.Erro ?? "Requisicao invalida."))
    };
}

internal sealed record RespostaSaude(string Servico, string Status);

internal sealed record RespostaServico(string Servico, string Responsabilidade);

internal sealed record RespostaErro(string Erro);

public partial class Program
{
}
