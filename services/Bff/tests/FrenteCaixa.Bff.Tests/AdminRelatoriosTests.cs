using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Bff.Api.Admin;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Bff.Tests;

public sealed class AdminRelatoriosTests
{
    [Fact]
    public async Task bff_admin_can_list_completed_sales()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.GetAsync("/admin/reports/sales");
        var vendas = await resposta.Content.ReadFromJsonAsync<AdminVendaRelatorioResponse[]>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(vendas);
        var venda = Assert.Single(vendas);
        Assert.Equal(factory.RelatoriosService.Venda.VendaId, venda.VendaId);
        Assert.Equal(35.90m, venda.ValorTotal);
        Assert.StartsWith("Bearer ", factory.RelatoriosService.UltimoAuthorizationHeader);
    }

    [Fact]
    public async Task bff_admin_can_get_financial_summary()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.GetAsync("/admin/reports/financial-summary");
        var resumo = await resposta.Content.ReadFromJsonAsync<AdminResumoFinanceiroResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(resumo);
        Assert.Equal(1, resumo.QuantidadeVendas);
        Assert.Equal(35.90m, resumo.ValorTotal);
        Assert.Contains(resumo.TotaisPorFormaPagamento, total =>
            total.FormaPagamento == "Dinheiro"
            && total.QuantidadeVendas == 1
            && total.ValorTotal == 35.90m);
    }

    [Fact]
    public async Task bff_seller_cannot_access_admin_reports()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync("/admin/reports/sales");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task bff_admin_reports_unavailable_returns_bad_gateway()
    {
        using var factory = new BffApiFactory();
        factory.RelatoriosService.MarcarIndisponivel();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.GetAsync("/admin/reports/sales");

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
