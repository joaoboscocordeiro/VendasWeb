using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Relatorios.Application.Relatorios.Contratos;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Relatorios.Tests;

public sealed class RelatoriosEndpointsTests
{
    [Fact]
    public async Task reports_admin_can_list_completed_sales()
    {
        using var factory = new RelatoriosApiFactory();
        var vendaProjetada = await factory.SemearVendaAsync();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.GetAsync("/reports/sales");
        var vendas = await resposta.Content.ReadFromJsonAsync<VendaRelatorioResponse[]>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(vendas);
        var venda = Assert.Single(vendas);
        Assert.Equal(vendaProjetada.VendaId, venda.VendaId);
        Assert.Equal(vendaProjetada.ValorTotal, venda.ValorTotal);
    }

    [Fact]
    public async Task reports_seller_cannot_access_global_sales_report()
    {
        using var factory = new RelatoriosApiFactory();
        await factory.SemearVendaAsync();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync("/reports/sales");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task reports_admin_can_get_financial_summary()
    {
        using var factory = new RelatoriosApiFactory();
        await factory.SemearVendaAsync("Dinheiro", 20m);
        await factory.SemearVendaAsync("Pix", 15.50m);
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.GetAsync("/reports/financial-summary");
        var resumo = await resposta.Content.ReadFromJsonAsync<ResumoFinanceiroResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(resumo);
        Assert.Equal(2, resumo.QuantidadeVendas);
        Assert.Equal(35.50m, resumo.ValorTotal);
        Assert.Contains(resumo.TotaisPorFormaPagamento, total =>
            total.FormaPagamento == "Dinheiro"
            && total.QuantidadeVendas == 1
            && total.ValorTotal == 20m);
        Assert.Contains(resumo.TotaisPorFormaPagamento, total =>
            total.FormaPagamento == "Pix"
            && total.QuantidadeVendas == 1
            && total.ValorTotal == 15.50m);
    }

    private static void Autenticar(HttpClient client, string perfil)
    {
        var token = CriarToken(perfil);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static string CriarToken(string perfil)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(RelatoriosApiFactory.JwtKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var agora = DateTimeOffset.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, $"{perfil.ToLowerInvariant()}@frentecaixa.local"),
            new Claim("role", perfil),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, agora.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            RelatoriosApiFactory.JwtIssuer,
            RelatoriosApiFactory.JwtAudience,
            claims,
            agora.UtcDateTime,
            agora.AddMinutes(15).UtcDateTime,
            credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
