using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FrenteCaixa.Bff.Api.Admin;
using FrenteCaixa.Bff.Api.Pdv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

GarantirConfiguracaoJwt(builder);

builder.Services.AddOpenApi();
builder.Services.Configure<PdvBootstrapOptions>(
    builder.Configuration.GetSection(PdvBootstrapOptions.Secao));
builder.Services.AddHttpClient<IBackendHealthClient, BackendHealthClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(2);
});
builder.Services.AddHttpClient<IPdvProdutosService, CatalogoProdutosPdvService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddHttpClient<IPdvCaixaService, CaixaPdvService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddHttpClient<IAdminRelatoriosService, AdminRelatoriosService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddHttpClient<IAdminProdutosService, AdminProdutosService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddScoped<IPdvBootstrapService, PdvBootstrapService>();
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
    options.AddPolicy("OperadorCaixa", policy => policy.RequireRole("ADM", "VENDEDOR"));
    options.AddPolicy("SomenteAdministrador", policy => policy.RequireRole("ADM"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var nomeServico = "BFF";

app.MapGet("/api/saude", () =>
    Results.Ok(new RespostaSaude(nomeServico, "Operacional")))
    .WithName("ObterSaude");

app.MapGet("/api/info", () =>
    Results.Ok(new RespostaServico(nomeServico, "Backend for Frontend do PDV")))
    .WithName("ObterInformacoesServico");

var pdv = app.MapGroup("/pdv")
    .RequireAuthorization("OperadorCaixa")
    .WithTags("PDV");

pdv.MapGet("/bootstrap", async (
        ClaimsPrincipal usuario,
        IPdvBootstrapService bootstrapService,
        CancellationToken cancellationToken) =>
    {
        var bootstrap = await bootstrapService.ObterAsync(usuario, cancellationToken);

        return Results.Ok(bootstrap);
    })
    .WithName("ObterBootstrapPdv");

pdv.MapGet("/products", async (
        string? term,
        HttpRequest request,
        IPdvProdutosService produtosService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var produtos = await produtosService.BuscarAsync(
                term,
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return Results.Ok(produtos);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Catalogo de produtos indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("BuscarProdutosPdv");

pdv.MapGet("/products/by-barcode/{ean}", async (
        string ean,
        HttpRequest request,
        IPdvProdutosService produtosService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var produto = await produtosService.ObterPorCodigoBarrasAsync(
                ean,
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return produto is null
                ? Results.NotFound(new RespostaErro("Produto ativo nao encontrado."))
                : Results.Ok(produto);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Catalogo de produtos indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("ObterProdutoPdvPorCodigoBarras");

pdv.MapGet("/cash-register/current", async (
        HttpRequest request,
        IPdvCaixaService caixaService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var caixa = await caixaService.ObterAtualAsync(
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return caixa is null
                ? Results.NotFound(new RespostaErro("Nenhum caixa aberto para o operador."))
                : Results.Ok(caixa);
        }
        catch (PdvCaixaHttpException ex)
        {
            return MapearFalhaCaixa(ex);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Servico de Caixa indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("ObterCaixaAtualPdv");

pdv.MapPost("/cash-register/open", async (
        AbrirCaixaPdvRequest abrirCaixa,
        HttpRequest request,
        IPdvCaixaService caixaService,
        CancellationToken cancellationToken) =>
    {
        if (abrirCaixa.ValorInicial < 0)
        {
            return Results.BadRequest(new RespostaErro("Valor inicial nao pode ser negativo."));
        }

        try
        {
            var caixa = await caixaService.AbrirAsync(
                abrirCaixa,
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return Results.Created($"/pdv/cash-register/current", caixa);
        }
        catch (PdvCaixaHttpException ex)
        {
            return MapearFalhaCaixa(ex);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Servico de Caixa indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("AbrirCaixaPdv");

pdv.MapGet("/cash-register/current/movements", async (
        HttpRequest request,
        IPdvCaixaService caixaService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var caixa = await caixaService.ObterAtualAsync(
                request.Headers.Authorization.ToString(),
                cancellationToken);

            if (caixa is null)
            {
                return Results.NotFound(new RespostaErro("Nenhum caixa aberto para o operador."));
            }

            var movimentacoes = await caixaService.ListarMovimentacoesAsync(
                caixa.Id,
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return Results.Ok(movimentacoes);
        }
        catch (PdvCaixaHttpException ex)
        {
            return MapearFalhaCaixa(ex);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Servico de Caixa indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("ListarMovimentacoesCaixaAtualPdv");

pdv.MapGet("/cash-register/current/summary", async (
        HttpRequest request,
        IPdvCaixaService caixaService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var caixa = await caixaService.ObterAtualAsync(
                request.Headers.Authorization.ToString(),
                cancellationToken);

            if (caixa is null)
            {
                return Results.NotFound(new RespostaErro("Nenhum caixa aberto para o operador."));
            }

            var resumo = await caixaService.ObterResumoAsync(
                caixa.Id,
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return Results.Ok(resumo);
        }
        catch (PdvCaixaHttpException ex)
        {
            return MapearFalhaCaixa(ex);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Servico de Caixa indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("ObterResumoCaixaAtualPdv");

pdv.MapPost("/cash-register/{id:guid}/close", async (
        Guid id,
        FecharCaixaPdvRequest fecharCaixa,
        HttpRequest request,
        IPdvCaixaService caixaService,
        CancellationToken cancellationToken) =>
    {
        if (fecharCaixa.ValorFechamento < 0)
        {
            return Results.BadRequest(new RespostaErro("Valor de fechamento nao pode ser negativo."));
        }

        try
        {
            var caixa = await caixaService.FecharAsync(
                id,
                fecharCaixa,
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return Results.Ok(caixa);
        }
        catch (PdvCaixaHttpException ex)
        {
            return MapearFalhaCaixa(ex);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Servico de Caixa indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("FecharCaixaPdv");

var adminReports = app.MapGroup("/admin/reports")
    .RequireAuthorization("SomenteAdministrador")
    .WithTags("Admin Relatorios");

adminReports.MapGet("/sales", async (
        HttpRequest request,
        IAdminRelatoriosService relatoriosService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var vendas = await relatoriosService.ListarVendasAsync(
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return Results.Ok(vendas);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Servico de Relatorios indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("ListarVendasConcluidasAdmin");

adminReports.MapGet("/financial-summary", async (
        HttpRequest request,
        IAdminRelatoriosService relatoriosService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var resumo = await relatoriosService.ObterResumoFinanceiroAsync(
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return Results.Ok(resumo);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Servico de Relatorios indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("ObterResumoFinanceiroAdmin");

var adminProducts = app.MapGroup("/admin/products")
    .RequireAuthorization("SomenteAdministrador")
    .WithTags("Admin Produtos");

adminProducts.MapGet("", async (
        string? term,
        bool? onlyActive,
        HttpRequest request,
        IAdminProdutosService produtosService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var produtos = await produtosService.ListarAsync(
                term,
                onlyActive == true,
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return Results.Ok(produtos);
        }
        catch (AdminProdutosHttpException ex)
        {
            return MapearFalhaProdutos(ex);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Catalogo de produtos indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("ListarProdutosAdmin");

adminProducts.MapGet("/{id:guid}", async (
        Guid id,
        HttpRequest request,
        IAdminProdutosService produtosService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var produto = await produtosService.ObterPorIdAsync(
                id,
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return produto is null
                ? Results.NotFound(new RespostaErro("Produto nao encontrado."))
                : Results.Ok(produto);
        }
        catch (AdminProdutosHttpException ex)
        {
            return MapearFalhaProdutos(ex);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Catalogo de produtos indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("ObterProdutoAdminPorId");

adminProducts.MapPost("", async (
        AdminProdutoRequest produto,
        HttpRequest request,
        IAdminProdutosService produtosService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var produtoCriado = await produtosService.CadastrarAsync(
                produto,
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return Results.Created($"/admin/products/{produtoCriado.Id}", produtoCriado);
        }
        catch (AdminProdutosHttpException ex)
        {
            return MapearFalhaProdutos(ex);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Catalogo de produtos indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("CadastrarProdutoAdmin");

adminProducts.MapPut("/{id:guid}", async (
        Guid id,
        AdminProdutoRequest produto,
        HttpRequest request,
        IAdminProdutosService produtosService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var produtoAtualizado = await produtosService.AtualizarAsync(
                id,
                produto,
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return Results.Ok(produtoAtualizado);
        }
        catch (AdminProdutosHttpException ex)
        {
            return MapearFalhaProdutos(ex);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Catalogo de produtos indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("AtualizarProdutoAdmin");

adminProducts.MapPatch("/{id:guid}/disable", async (
        Guid id,
        HttpRequest request,
        IAdminProdutosService produtosService,
        CancellationToken cancellationToken) =>
    {
        try
        {
            await produtosService.InativarAsync(
                id,
                request.Headers.Authorization.ToString(),
                cancellationToken);

            return Results.NoContent();
        }
        catch (AdminProdutosHttpException ex)
        {
            return MapearFalhaProdutos(ex);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "Catalogo de produtos indisponivel.",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("InativarProdutoAdmin");

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

static IResult MapearFalhaCaixa(PdvCaixaHttpException ex)
{
    return ex.StatusCode switch
    {
        HttpStatusCode.BadRequest => Results.BadRequest(new RespostaErro("Requisicao invalida para o Caixa.")),
        HttpStatusCode.Conflict => Results.Conflict(new RespostaErro("Conflito na operacao de caixa.")),
        HttpStatusCode.NotFound => Results.NotFound(new RespostaErro("Nenhum caixa aberto para o operador.")),
        _ => Results.Problem(
            title: "Servico de Caixa retornou erro.",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway)
    };
}

static IResult MapearFalhaProdutos(AdminProdutosHttpException ex)
{
    return ex.StatusCode switch
    {
        HttpStatusCode.BadRequest => Results.BadRequest(new RespostaErro(ex.Message)),
        HttpStatusCode.Conflict => Results.Conflict(new RespostaErro(ex.Message)),
        HttpStatusCode.NotFound => Results.NotFound(new RespostaErro(ex.Message)),
        _ => Results.Problem(
            title: "Catalogo de produtos retornou erro.",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway)
    };
}

internal sealed record RespostaSaude(string Servico, string Status);

internal sealed record RespostaServico(string Servico, string Responsabilidade);

internal sealed record RespostaErro(string Erro);

internal sealed record ConfiguracaoJwt(string Issuer, string Audience, string Chave);

public partial class Program
{
}
