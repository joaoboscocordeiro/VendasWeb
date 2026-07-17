using System.Security.Cryptography;
using System.Text;
using FrenteCaixa.CatalogoProdutos.Application.Produtos;
using FrenteCaixa.CatalogoProdutos.Application.Produtos.Contratos;
using FrenteCaixa.CatalogoProdutos.Application.Produtos.Interfaces;
using FrenteCaixa.CatalogoProdutos.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

GarantirConfiguracaoJwt(builder);

builder.Services.AddOpenApi();
builder.Services.AdicionarInfraestruturaCatalogoProdutos(builder.Configuration);
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

var nomeServico = "CatalogoProdutos";

app.MapGet("/api/saude", () =>
    Results.Ok(new RespostaSaude(nomeServico, "Operacional")))
    .WithName("ObterSaude");

app.MapGet("/api/info", () =>
    Results.Ok(new RespostaServico(nomeServico, "Cadastro, consulta e manutencao de produtos")))
    .WithName("ObterInformacoesServico");

var produtos = app.MapGroup("/products")
    .WithTags("Produtos");

produtos.MapPost("", async (
        CadastrarProdutoRequest request,
        IServicoProdutos servicoProdutos,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoProdutos.CadastrarAsync(request, cancellationToken);

        return resultado.Sucesso
            ? Results.Created($"/products/{resultado.Valor!.Id}", resultado.Valor)
            : MapearFalha(resultado);
    })
    .RequireAuthorization("SomenteAdministrador")
    .WithName("CadastrarProduto");

produtos.MapGet("", async (
        string? term,
        bool? onlyActive,
        IServicoProdutos servicoProdutos,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoProdutos.ListarAsync(term, onlyActive == true, cancellationToken);

        return Results.Ok(resultado);
    })
    .RequireAuthorization("UsuarioAutenticado")
    .WithName("ListarProdutos");

produtos.MapGet("/{id:guid}", async (
        Guid id,
        IServicoProdutos servicoProdutos,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoProdutos.ObterPorIdAsync(id, cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : MapearFalha(resultado);
    })
    .RequireAuthorization("UsuarioAutenticado")
    .WithName("ObterProdutoPorId");

produtos.MapGet("/by-barcode/{ean}", async (
        string ean,
        IServicoProdutos servicoProdutos,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoProdutos.ObterPorCodigoBarrasAsync(ean, cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : MapearFalha(resultado);
    })
    .RequireAuthorization("UsuarioAutenticado")
    .WithName("ObterProdutoPorCodigoBarras");

produtos.MapPut("/{id:guid}", async (
        Guid id,
        AtualizarProdutoRequest request,
        IServicoProdutos servicoProdutos,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoProdutos.AtualizarAsync(id, request, cancellationToken);

        return resultado.Sucesso
            ? Results.Ok(resultado.Valor)
            : MapearFalha(resultado);
    })
    .RequireAuthorization("SomenteAdministrador")
    .WithName("AtualizarProduto");

produtos.MapPatch("/{id:guid}/disable", async (
        Guid id,
        IServicoProdutos servicoProdutos,
        CancellationToken cancellationToken) =>
    {
        var resultado = await servicoProdutos.InativarAsync(id, cancellationToken);

        return resultado.Sucesso
            ? Results.NoContent()
            : MapearFalha(resultado);
    })
    .RequireAuthorization("SomenteAdministrador")
    .WithName("InativarProduto");

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
