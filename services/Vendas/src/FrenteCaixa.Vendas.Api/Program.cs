using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FrenteCaixa.Vendas.Application.Vendas;
using FrenteCaixa.Vendas.Application.Vendas.Contratos;
using FrenteCaixa.Vendas.Application.Vendas.Interfaces;
using FrenteCaixa.Vendas.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

GarantirConfiguracaoJwt(builder);

builder.Services.AddOpenApi();
builder.Services.AdicionarInfraestruturaVendas(builder.Configuration);
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

var nomeServico = "Vendas";

app.MapGet("/api/saude", () =>
    Results.Ok(new RespostaSaude(nomeServico, "Operacional")))
    .WithName("ObterSaude");

app.MapGet("/api/info", () =>
    Results.Ok(new RespostaServico(nomeServico, "Venda, itens e finalizacao do PDV")))
    .WithName("ObterInformacoesServico");

var vendas = app.MapGroup("/sales")
    .RequireAuthorization("OperadorCaixa")
    .WithTags("Vendas");

vendas.MapPost("", async (
        IniciarVendaRequest request,
        ClaimsPrincipal usuario,
        IServicoVendas servicoVendas,
        CancellationToken cancellationToken) =>
    {
        var operadorId = ObterOperadorId(usuario);

        if (operadorId is null)
        {
            return Results.BadRequest(new RespostaErro("Token nao contem operador valido."));
        }

        var resultado = await servicoVendas.IniciarAsync(
            operadorId.Value,
            request,
            cancellationToken);

        return resultado.Sucesso
            ? Results.Created($"/sales/{resultado.Valor!.Id}", resultado.Valor)
            : MapearFalha(resultado);
    })
    .WithName("IniciarVenda");

vendas.MapGet("/{id:guid}", async (
        Guid id,
        ClaimsPrincipal usuario,
        IServicoVendas servicoVendas,
        CancellationToken cancellationToken) =>
    {
        var operadorId = ObterOperadorId(usuario);

        if (operadorId is null)
        {
            return Results.BadRequest(new RespostaErro("Token nao contem operador valido."));
        }

        var resultado = await servicoVendas.ObterAsync(operadorId.Value, id, cancellationToken);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : MapearFalha(resultado);
    })
    .WithName("ObterVenda");

vendas.MapPost("/{id:guid}/items", async (
        Guid id,
        AdicionarItemVendaRequest request,
        ClaimsPrincipal usuario,
        IServicoVendas servicoVendas,
        CancellationToken cancellationToken) =>
    {
        var operadorId = ObterOperadorId(usuario);

        if (operadorId is null)
        {
            return Results.BadRequest(new RespostaErro("Token nao contem operador valido."));
        }

        var resultado = await servicoVendas.AdicionarItemAsync(
            operadorId.Value,
            id,
            request,
            cancellationToken);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : MapearFalha(resultado);
    })
    .WithName("AdicionarItemVenda");

vendas.MapDelete("/{id:guid}/items/{itemId:guid}", async (
        Guid id,
        Guid itemId,
        ClaimsPrincipal usuario,
        IServicoVendas servicoVendas,
        CancellationToken cancellationToken) =>
    {
        var operadorId = ObterOperadorId(usuario);

        if (operadorId is null)
        {
            return Results.BadRequest(new RespostaErro("Token nao contem operador valido."));
        }

        var resultado = await servicoVendas.RemoverItemAsync(
            operadorId.Value,
            id,
            itemId,
            cancellationToken);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : MapearFalha(resultado);
    })
    .WithName("RemoverItemVenda");

vendas.MapPost("/{id:guid}/checkout", async (
        Guid id,
        FinalizarVendaRequest request,
        ClaimsPrincipal usuario,
        HttpContext httpContext,
        IServicoVendas servicoVendas,
        CancellationToken cancellationToken) =>
    {
        var operadorId = ObterOperadorId(usuario);

        if (operadorId is null)
        {
            return Results.BadRequest(new RespostaErro("Token nao contem operador valido."));
        }

        var accessToken = ObterAccessToken(httpContext);

        if (accessToken is null)
        {
            return Results.BadRequest(new RespostaErro("Token de acesso e obrigatorio."));
        }

        var resultado = await servicoVendas.FinalizarAsync(
            operadorId.Value,
            id,
            request,
            accessToken,
            cancellationToken);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : MapearFalha(resultado);
    })
    .WithName("FinalizarVenda");

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

static string? ObterAccessToken(HttpContext httpContext)
{
    var authorization = httpContext.Request.Headers.Authorization.ToString();
    const string prefixo = "Bearer ";

    return authorization.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase)
        ? authorization[prefixo.Length..].Trim()
        : null;
}

static IResult MapearFalha<T>(ResultadoOperacao<T> resultado)
{
    return resultado.CodigoErro switch
    {
        CodigoErroOperacao.Validacao => Results.BadRequest(new RespostaErro(resultado.Erro ?? "Requisicao invalida.")),
        CodigoErroOperacao.Conflito => Results.Conflict(new RespostaErro(resultado.Erro ?? "Conflito.")),
        CodigoErroOperacao.NaoEncontrado => Results.NotFound(new RespostaErro(resultado.Erro ?? "Recurso nao encontrado.")),
        CodigoErroOperacao.DependenciaIndisponivel => Results.Problem(
            resultado.Erro ?? "Dependencia indisponivel.",
            statusCode: StatusCodes.Status503ServiceUnavailable),
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
