using System.Security.Cryptography;
using System.Text;
using FrenteCaixa.Estoque.Application.Estoques;
using FrenteCaixa.Estoque.Application.Estoques.Contratos;
using FrenteCaixa.Estoque.Application.Estoques.Interfaces;
using FrenteCaixa.Estoque.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

GarantirConfiguracaoJwt(builder);

builder.Services.AddOpenApi();
builder.Services.AdicionarInfraestruturaEstoque(builder.Configuration);
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

var nomeServico = "Estoque";

app.MapGet("/api/saude", () =>
    Results.Ok(new RespostaSaude(nomeServico, "Operacional")))
    .WithName("ObterSaude");

app.MapGet("/api/info", () =>
    Results.Ok(new RespostaServico(nomeServico, "Saldos, movimentos e deducao de estoque")))
    .WithName("ObterInformacoesServico");

var estoque = app.MapGroup("/stock")
    .WithTags("Estoque");

estoque.MapGet("/products/{productId:guid}", async (
        Guid productId,
        IServicoEstoque servicoEstoque,
        CancellationToken cancellationToken) =>
    {
        var saldo = await servicoEstoque.ObterSaldoAsync(productId, cancellationToken);

        return Results.Ok(saldo);
    })
    .RequireAuthorization("UsuarioAutenticado")
    .WithName("ObterSaldoProduto");

estoque.MapGet("/products/{productId:guid}/movements", async (
        Guid productId,
        IServicoEstoque servicoEstoque,
        CancellationToken cancellationToken) =>
    {
        var movimentacoes = await servicoEstoque.ListarMovimentacoesAsync(productId, cancellationToken);

        return Results.Ok(movimentacoes);
    })
    .RequireAuthorization("UsuarioAutenticado")
    .WithName("ListarMovimentacoesProduto");

estoque.MapPost("/adjustments", async (
        RegistrarAjusteEstoqueRequest request,
        IServicoEstoque servicoEstoque,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoEstoque.RegistrarAjusteAsync(request, cancellationToken);

        return resultado.Sucesso
            ? Results.Created($"/stock/products/{resultado.Valor!.ProdutoId}/movements/{resultado.Valor.Id}", resultado.Valor)
            : MapearFalha(resultado);
    })
    .RequireAuthorization("SomenteAdministrador")
    .WithName("RegistrarAjusteEstoque");

estoque.MapPost("/deductions", async (
        DeducaoEstoqueRequest request,
        IServicoEstoque servicoEstoque,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoEstoque.RegistrarDeducaoVendaAsync(request, cancellationToken);

        return resultado.Sucesso
            ? Results.Created($"/stock/deductions/{request.VendaId}", resultado.Valor)
            : MapearFalha(resultado);
    })
    .RequireAuthorization("OperadorCaixa")
    .WithName("RegistrarDeducaoVenda");

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
