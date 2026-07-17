using System.Security.Cryptography;
using System.Text;
using FrenteCaixa.Pagamentos.Application.Pagamentos;
using FrenteCaixa.Pagamentos.Application.Pagamentos.Contratos;
using FrenteCaixa.Pagamentos.Application.Pagamentos.Interfaces;
using FrenteCaixa.Pagamentos.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

GarantirConfiguracaoJwt(builder);

builder.Services.AddOpenApi();
builder.Services.AdicionarInfraestruturaPagamentos(builder.Configuration);
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

var nomeServico = "Pagamentos";

app.MapGet("/api/saude", () =>
    Results.Ok(new RespostaSaude(nomeServico, "Operacional")))
    .WithName("ObterSaude");

app.MapGet("/api/info", () =>
    Results.Ok(new RespostaServico(nomeServico, "Registro e validacao de pagamentos")))
    .WithName("ObterInformacoesServico");

var pagamentos = app.MapGroup("/payments")
    .RequireAuthorization("OperadorCaixa")
    .WithTags("Pagamentos");

pagamentos.MapPost("", async (
        RegistrarPagamentoRequest request,
        IServicoPagamentos servicoPagamentos,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoPagamentos.RegistrarAsync(request, cancellationToken);

        return resultado.Sucesso
            ? Results.Created($"/payments/{resultado.Valor!.Id}", resultado.Valor)
            : MapearFalha(resultado);
    })
    .WithName("RegistrarPagamento");

pagamentos.MapGet("/{id:guid}", async (
        Guid id,
        IServicoPagamentos servicoPagamentos,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoPagamentos.ObterAsync(id, cancellationToken);

        return resultado.Sucesso ? Results.Ok(resultado.Valor) : MapearFalha(resultado);
    })
    .WithName("ObterPagamento");

pagamentos.MapGet("/by-sale/{saleId:guid}", async (
        Guid saleId,
        IServicoPagamentos servicoPagamentos,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoPagamentos.ListarPorVendaAsync(saleId, cancellationToken);

        return Results.Ok(resultado);
    })
    .WithName("ListarPagamentosPorVenda");

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
