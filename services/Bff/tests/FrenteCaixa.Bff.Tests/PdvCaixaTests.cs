using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Bff.Api.Pdv;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Bff.Tests;

public sealed class PdvCaixaTests
{
    [Fact]
    public async Task bff_pdv_cash_register_current_returns_operator_register()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync("/pdv/cash-register/current");
        var caixa = await resposta.Content.ReadFromJsonAsync<PdvCaixaResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(caixa);
        Assert.Equal("Aberto", caixa.Status);
        Assert.Equal(25m, caixa.ValorInicial);
        Assert.StartsWith("Bearer ", factory.CaixaService.UltimoAuthorizationHeader);
    }

    [Fact]
    public async Task bff_pdv_cash_register_current_returns_not_found_when_missing()
    {
        using var factory = new BffApiFactory();
        factory.CaixaService.CaixaAtual = null;
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync("/pdv/cash-register/current");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task bff_pdv_cash_register_open_posts_initial_value()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            "/pdv/cash-register/open",
            new AbrirCaixaPdvRequest(50m));
        var caixa = await resposta.Content.ReadFromJsonAsync<PdvCaixaResponse>();

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(caixa);
        Assert.Equal("Aberto", caixa.Status);
        Assert.Equal(50m, caixa.ValorInicial);
        Assert.Equal(50m, factory.CaixaService.UltimoValorInicial);
        Assert.StartsWith("Bearer ", factory.CaixaService.UltimoAuthorizationHeader);
    }

    [Fact]
    public async Task bff_pdv_cash_register_close_posts_closing_value()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            $"/pdv/cash-register/{factory.CaixaService.CaixaAtual!.Id}/close",
            new FecharCaixaPdvRequest(125.50m));
        var caixa = await resposta.Content.ReadFromJsonAsync<PdvCaixaResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(caixa);
        Assert.Equal("Fechado", caixa.Status);
        Assert.Equal(125.50m, caixa.ValorFechamento);
        Assert.Equal(125.50m, factory.CaixaService.UltimoValorFechamento);
        Assert.Equal(caixa.Id, factory.CaixaService.UltimoCaixaId);
        Assert.StartsWith("Bearer ", factory.CaixaService.UltimoAuthorizationHeader);
    }

    [Fact]
    public async Task bff_pdv_cash_register_current_movements_returns_movements()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync("/pdv/cash-register/current/movements");
        var movimentacoes = await resposta.Content.ReadFromJsonAsync<PdvMovimentacaoCaixaResponse[]>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(movimentacoes);
        var movimentacao = Assert.Single(movimentacoes);
        Assert.Equal("Abertura", movimentacao.Tipo);
        Assert.Equal(factory.CaixaService.CaixaAtual!.Id, movimentacao.CaixaId);
        Assert.StartsWith("Bearer ", factory.CaixaService.UltimoAuthorizationHeader);
    }

    [Fact]
    public async Task bff_pdv_cash_register_current_summary_returns_summary()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync("/pdv/cash-register/current/summary");
        var resumo = await resposta.Content.ReadFromJsonAsync<PdvResumoCaixaResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(resumo);
        Assert.Equal(2, resumo.QuantidadeVendas);
        Assert.Equal(55m, resumo.TotalVendido);
        Assert.Equal(65m, resumo.DinheiroEsperado);
        Assert.Contains(resumo.TotaisPorFormaPagamento, total =>
            total.FormaPagamento == "Dinheiro" && total.Total == 40m);
        Assert.Equal(factory.CaixaService.CaixaAtual!.Id, factory.CaixaService.UltimoCaixaId);
        Assert.StartsWith("Bearer ", factory.CaixaService.UltimoAuthorizationHeader);
    }

    [Fact]
    public async Task bff_pdv_cash_register_current_summary_missing_cash_register_returns_not_found()
    {
        using var factory = new BffApiFactory();
        factory.CaixaService.CaixaAtual = null;
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync("/pdv/cash-register/current/summary");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task bff_pdv_cash_register_close_rejects_negative_value()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            $"/pdv/cash-register/{factory.CaixaService.CaixaAtual!.Id}/close",
            new FecharCaixaPdvRequest(-1m));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Null(factory.CaixaService.UltimoValorFechamento);
    }

    [Fact]
    public async Task bff_pdv_cash_register_requires_authentication()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();

        var resposta = await client.GetAsync("/pdv/cash-register/current");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
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
