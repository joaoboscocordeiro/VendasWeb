using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Bff.Api.Pdv;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Bff.Tests;

public sealed class PdvBootstrapTests
{
    [Fact]
    public async Task bff_bootstrap_returns_operator_context()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        var usuarioId = Guid.NewGuid();
        Autenticar(client, usuarioId, "Vendedor Teste", "vendedor@frentecaixa.local", "VENDEDOR");

        var resposta = await client.GetAsync("/pdv/bootstrap");
        var bootstrap = await resposta.Content.ReadFromJsonAsync<PdvBootstrapResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(bootstrap);
        Assert.Equal(usuarioId, bootstrap.Usuario.Id);
        Assert.Equal("Vendedor Teste", bootstrap.Usuario.Nome);
        Assert.Equal("vendedor@frentecaixa.local", bootstrap.Usuario.Email);
        Assert.Equal("VENDEDOR", bootstrap.Usuario.Perfil);
        Assert.Equal("BRL", bootstrap.Configuracao.Moeda);
        Assert.False(bootstrap.Configuracao.PermiteVendaSemEstoque);
        Assert.Contains(bootstrap.Atalhos, atalho => atalho.Codigo == "caixa-atual");
        Assert.Contains(bootstrap.Servicos, servico => servico.Nome == "CatalogoProdutos");
        Assert.Contains(bootstrap.Servicos, servico => servico.Nome == "Estoque");
        Assert.All(bootstrap.Servicos, servico => Assert.Equal("Operacional", servico.Status));
    }

    [Fact]
    public async Task bff_bootstrap_requires_authentication()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();

        var resposta = await client.GetAsync("/pdv/bootstrap");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task bff_bootstrap_rejects_unauthorized_role()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, Guid.NewGuid(), "Suporte", "suporte@frentecaixa.local", "SUPORTE");

        var resposta = await client.GetAsync("/pdv/bootstrap");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task bff_bootstrap_marks_unavailable_backend_without_5xx()
    {
        using var factory = new BffApiFactory();
        factory.BackendHealthClient.MarcarIndisponivel("Estoque");
        var client = factory.CreateClient();
        Autenticar(client, Guid.NewGuid(), "Administrador", "admin@frentecaixa.local", "ADM");

        var resposta = await client.GetAsync("/pdv/bootstrap");
        var bootstrap = await resposta.Content.ReadFromJsonAsync<PdvBootstrapResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(bootstrap);
        Assert.Contains(bootstrap.Servicos, servico =>
            servico.Nome == "Estoque" && servico.Status == "Indisponivel");
        Assert.Contains(bootstrap.Servicos, servico =>
            servico.Nome == "CatalogoProdutos" && servico.Status == "Operacional");
    }

    private static void Autenticar(
        HttpClient client,
        Guid usuarioId,
        string nome,
        string email,
        string perfil)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CriarToken(usuarioId, nome, email, perfil));
    }

    private static string CriarToken(Guid usuarioId, string nome, string email, string perfil)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(BffApiFactory.JwtKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var agora = DateTimeOffset.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim("name", nome),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("role", perfil),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, agora.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            BffApiFactory.JwtIssuer,
            BffApiFactory.JwtAudience,
            claims,
            agora.UtcDateTime,
            agora.AddMinutes(15).UtcDateTime,
            credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
