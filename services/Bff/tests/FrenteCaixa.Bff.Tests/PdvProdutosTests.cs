using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Bff.Api.Pdv;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Bff.Tests;

public sealed class PdvProdutosTests
{
    [Fact]
    public async Task bff_pdv_products_returns_active_catalog_products()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync("/pdv/products?term=cafe");
        var produtos = await resposta.Content.ReadFromJsonAsync<PdvProdutoResponse[]>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(produtos);
        var produto = Assert.Single(produtos);
        Assert.Equal("Cafe Torrado 500g", produto.Descricao);
        Assert.Equal("7891234567895", produto.CodigoBarrasEan);
        Assert.Equal(14.90m, produto.PrecoVenda);
        Assert.True(produto.Ativo);
        Assert.StartsWith("Bearer ", factory.ProdutosService.UltimoAuthorizationHeader);
    }

    [Fact]
    public async Task bff_pdv_products_requires_authentication()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();

        var resposta = await client.GetAsync("/pdv/products?term=cafe");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task bff_pdv_product_by_barcode_returns_not_found_when_missing()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync("/pdv/products/by-barcode/7890000000000");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    private static void Autenticar(HttpClient client, string perfil)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CriarToken(perfil));
    }

    private static string CriarToken(string perfil)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(BffApiFactory.JwtKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var agora = DateTimeOffset.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim("name", "Operador Teste"),
            new Claim(JwtRegisteredClaimNames.Email, $"{perfil.ToLowerInvariant()}@frentecaixa.local"),
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
