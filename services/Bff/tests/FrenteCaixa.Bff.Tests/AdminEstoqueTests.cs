using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Bff.Api.Admin;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Bff.Tests;

public sealed class AdminEstoqueTests
{
    [Fact]
    public async Task bff_admin_can_get_inventory_balance_and_movements()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var respostaSaldo = await client.GetAsync(
            $"/admin/inventory/products/{factory.AdminEstoqueService.ProdutoId}");
        var saldo = await respostaSaldo.Content.ReadFromJsonAsync<AdminSaldoEstoqueResponse>();

        var respostaMovimentacoes = await client.GetAsync(
            $"/admin/inventory/products/{factory.AdminEstoqueService.ProdutoId}/movements");
        var movimentacoes = await respostaMovimentacoes.Content
            .ReadFromJsonAsync<AdminMovimentacaoEstoqueResponse[]>();

        Assert.Equal(HttpStatusCode.OK, respostaSaldo.StatusCode);
        Assert.NotNull(saldo);
        Assert.Equal(factory.AdminEstoqueService.ProdutoId, saldo.ProdutoId);
        Assert.Equal(12.5m, saldo.QuantidadeDisponivel);

        Assert.Equal(HttpStatusCode.OK, respostaMovimentacoes.StatusCode);
        Assert.NotNull(movimentacoes);
        var movimentacao = Assert.Single(movimentacoes);
        Assert.Equal("Entrada", movimentacao.Tipo);
        Assert.Equal(5m, movimentacao.Quantidade);
        Assert.StartsWith("Bearer ", factory.AdminEstoqueService.UltimoAuthorizationHeader);
    }

    [Fact]
    public async Task bff_admin_can_register_inventory_adjustment()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");
        var ajuste = new AdminAjusteEstoqueRequest(
            factory.AdminEstoqueService.ProdutoId,
            "Saida",
            2m,
            "Quebra operacional");

        var resposta = await client.PostAsJsonAsync("/admin/inventory/adjustments", ajuste);
        var movimentacao = await resposta.Content.ReadFromJsonAsync<AdminMovimentacaoEstoqueResponse>();

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(movimentacao);
        Assert.Equal(ajuste.ProdutoId, movimentacao.ProdutoId);
        Assert.Equal(ajuste.Tipo, movimentacao.Tipo);
        Assert.Equal(ajuste.Quantidade, movimentacao.Quantidade);
        Assert.Equal(ajuste.Motivo, movimentacao.Motivo);
        Assert.Equal(ajuste, factory.AdminEstoqueService.UltimoAjuste);
        Assert.StartsWith("Bearer ", factory.AdminEstoqueService.UltimoAuthorizationHeader);
    }

    [Fact]
    public async Task bff_seller_cannot_access_admin_inventory()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync(
            $"/admin/inventory/products/{factory.AdminEstoqueService.ProdutoId}");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task bff_admin_inventory_unavailable_returns_bad_gateway()
    {
        using var factory = new BffApiFactory();
        factory.AdminEstoqueService.MarcarIndisponivel();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.GetAsync(
            $"/admin/inventory/products/{factory.AdminEstoqueService.ProdutoId}");

        Assert.Equal(HttpStatusCode.BadGateway, resposta.StatusCode);
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
