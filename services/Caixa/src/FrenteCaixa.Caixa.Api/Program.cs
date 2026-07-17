using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FrenteCaixa.Caixa.Application.Caixas;
using FrenteCaixa.Caixa.Application.Caixas.Contratos;
using FrenteCaixa.Caixa.Application.Caixas.Interfaces;
using FrenteCaixa.Caixa.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

GarantirConfiguracaoJwt(builder);

builder.Services.AddOpenApi();
builder.Services.AdicionarInfraestruturaCaixa(builder.Configuration);
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
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UsuarioAutenticado", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("SomenteAdministrador", policy => policy.RequireRole("ADM"));
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

var nomeServico = "Caixa";

app.MapGet("/api/saude", () =>
    Results.Ok(new RespostaSaude(nomeServico, "Operacional")))
    .WithName("ObterSaude");

app.MapGet("/api/info", () =>
    Results.Ok(new RespostaServico(nomeServico, "Abertura, operacao e fechamento de caixa")))
    .WithName("ObterInformacoesServico");

var caixas = app.MapGroup("/cash-registers")
    .WithTags("Caixa");

caixas.MapPost("/open", async (
        AbrirCaixaRequest request,
        ClaimsPrincipal usuario,
        IServicoCaixa servicoCaixa,
        CancellationToken cancellationToken) =>
    {
        var operadorId = ObterOperadorId(usuario);

        if (operadorId is null)
        {
            return Results.BadRequest(new RespostaErro("Token nao contem operador valido."));
        }

        var resultado = await servicoCaixa.AbrirAsync(operadorId.Value, request, cancellationToken);

        return resultado.Sucesso
            ? Results.Created($"/cash-registers/{resultado.Valor!.Id}", resultado.Valor)
            : MapearFalha(resultado);
    })
    .RequireAuthorization("OperadorCaixa")
    .WithName("AbrirCaixa");

caixas.MapGet("/current", async (
        ClaimsPrincipal usuario,
        IServicoCaixa servicoCaixa,
        CancellationToken cancellationToken) =>
    {
        var operadorId = ObterOperadorId(usuario);

        if (operadorId is null)
        {
            return Results.BadRequest(new RespostaErro("Token nao contem operador valido."));
        }

        var resultado = await servicoCaixa.ObterAtualAsync(operadorId.Value, cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : MapearFalha(resultado);
    })
    .RequireAuthorization("OperadorCaixa")
    .WithName("ObterCaixaAtual");

caixas.MapPost("/{id:guid}/close", async (
        Guid id,
        FecharCaixaRequest request,
        ClaimsPrincipal usuario,
        IServicoCaixa servicoCaixa,
        CancellationToken cancellationToken) =>
    {
        var operadorId = ObterOperadorId(usuario);

        if (operadorId is null)
        {
            return Results.BadRequest(new RespostaErro("Token nao contem operador valido."));
        }

        var resultado = await servicoCaixa.FecharAsync(operadorId.Value, id, request, cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : MapearFalha(resultado);
    })
    .RequireAuthorization("OperadorCaixa")
    .WithName("FecharCaixa");

caixas.MapGet("/{id:guid}/movements", async (
        Guid id,
        ClaimsPrincipal usuario,
        IServicoCaixa servicoCaixa,
        CancellationToken cancellationToken) =>
    {
        var operadorId = ObterOperadorId(usuario);

        if (operadorId is null)
        {
            return Results.BadRequest(new RespostaErro("Token nao contem operador valido."));
        }

        var resultado = await servicoCaixa.ListarMovimentacoesAsync(operadorId.Value, id, cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : MapearFalha(resultado);
    })
    .RequireAuthorization("OperadorCaixa")
    .WithName("ListarMovimentacoesCaixa");

caixas.MapGet("/{id:guid}/summary", async (
        Guid id,
        ClaimsPrincipal usuario,
        IServicoCaixa servicoCaixa,
        CancellationToken cancellationToken) =>
    {
        var operadorId = ObterOperadorId(usuario);

        if (operadorId is null)
        {
            return Results.BadRequest(new RespostaErro("Token nao contem operador valido."));
        }

        var resultado = await servicoCaixa.ObterResumoAsync(operadorId.Value, id, cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : MapearFalha(resultado);
    })
    .RequireAuthorization("OperadorCaixa")
    .WithName("ObterResumoCaixa");

app.Run();

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

    return new ConfiguracaoJwt(
        secaoJwt["Issuer"] ?? "FrenteCaixa.Identidade",
        secaoJwt["Audience"] ?? "FrenteCaixa.Backend",
        secaoJwt["Chave"] ?? string.Empty);
}

static Guid? ObterOperadorId(ClaimsPrincipal usuario)
{
    var operador = usuario.FindFirstValue("sub") ?? usuario.FindFirstValue(ClaimTypes.NameIdentifier);

    return Guid.TryParse(operador, out var operadorId)
        ? operadorId
        : null;
}

static IResult MapearFalha<T>(ResultadoOperacao<T> resultado)
{
    return resultado.CodigoErro switch
    {
        CodigoErroOperacao.Validacao => Results.BadRequest(new RespostaErro(resultado.Erro ?? "Requisicao invalida.")),
        CodigoErroOperacao.Conflito => Results.Conflict(new RespostaErro(resultado.Erro ?? "Conflito.")),
        CodigoErroOperacao.NaoEncontrado => Results.NotFound(new RespostaErro(resultado.Erro ?? "Recurso nao encontrado.")),
        _ => Results.BadRequest(new RespostaErro(resultado.Erro ?? "Requisicao invalida."))
    };
}

internal sealed record RespostaSaude(string Servico, string Status);

internal sealed record RespostaServico(string Servico, string Responsabilidade);

internal sealed record RespostaErro(string Erro);

internal sealed record ConfiguracaoJwt(string Issuer, string Audience, string Chave);

public partial class Program
{
}
