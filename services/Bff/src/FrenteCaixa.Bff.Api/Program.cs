using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

internal sealed record RespostaSaude(string Servico, string Status);

internal sealed record RespostaServico(string Servico, string Responsabilidade);

internal sealed record ConfiguracaoJwt(string Issuer, string Audience, string Chave);

public partial class Program
{
}
