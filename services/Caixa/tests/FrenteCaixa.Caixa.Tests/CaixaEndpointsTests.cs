using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Caixa.Application.Caixas.Contratos;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Caixa.Tests;

public sealed class CaixaEndpointsTests
{
    [Fact]
    public async Task cash_operator_can_open_register()
    {
        using var factory = new CaixaApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR", CaixaApiFactory.OperadorPadraoId);

        var resposta = await client.PostAsJsonAsync(
            "/cash-registers/open",
            new AbrirCaixaRequest(150m));
        var caixa = await resposta.Content.ReadFromJsonAsync<CaixaOperacionalResponse>();

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(caixa);
        Assert.Equal("Aberto", caixa.Status);
        Assert.Equal(150m, caixa.ValorInicial);
    }

    [Fact]
    public async Task cash_open_rejects_duplicate_open_register()
    {
        using var factory = new CaixaApiFactory();
        await factory.SemearCaixaAbertoAsync();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR", CaixaApiFactory.OperadorPadraoId);

        var resposta = await client.PostAsJsonAsync(
            "/cash-registers/open",
            new AbrirCaixaRequest(10m));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task cash_open_rejects_negative_initial_value()
    {
        using var factory = new CaixaApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR", CaixaApiFactory.OperadorPadraoId);

        var resposta = await client.PostAsJsonAsync(
            "/cash-registers/open",
            new AbrirCaixaRequest(-1m));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task cash_operator_can_get_current_register()
    {
        using var factory = new CaixaApiFactory();
        await factory.SemearCaixaAbertoAsync();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR", CaixaApiFactory.OperadorPadraoId);

        var resposta = await client.GetAsync("/cash-registers/current");
        var caixa = await resposta.Content.ReadFromJsonAsync<CaixaOperacionalResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(caixa);
        Assert.Equal("Aberto", caixa.Status);
    }

    [Fact]
    public async Task cash_current_missing_returns_not_found()
    {
        using var factory = new CaixaApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR", CaixaApiFactory.OperadorPadraoId);

        var resposta = await client.GetAsync("/cash-registers/current");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task cash_operator_can_close_register()
    {
        using var factory = new CaixaApiFactory();
        var caixaAberto = await factory.SemearCaixaAbertoAsync();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR", CaixaApiFactory.OperadorPadraoId);

        var resposta = await client.PostAsJsonAsync(
            $"/cash-registers/{caixaAberto.Id}/close",
            new FecharCaixaRequest(175m));
        var caixa = await resposta.Content.ReadFromJsonAsync<CaixaOperacionalResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(caixa);
        Assert.Equal("Fechado", caixa.Status);
        Assert.Equal(175m, caixa.ValorFechamento);
    }

    [Fact]
    public async Task cash_close_rejects_other_operator_register()
    {
        using var factory = new CaixaApiFactory();
        var caixaAberto = await factory.SemearCaixaAbertoAsync(CaixaApiFactory.OutroOperadorId);
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR", CaixaApiFactory.OperadorPadraoId);

        var resposta = await client.PostAsJsonAsync(
            $"/cash-registers/{caixaAberto.Id}/close",
            new FecharCaixaRequest(175m));

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task cash_operator_can_list_register_movements()
    {
        using var factory = new CaixaApiFactory();
        var caixaAberto = await factory.SemearCaixaAbertoAsync();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR", CaixaApiFactory.OperadorPadraoId);

        var resposta = await client.GetAsync($"/cash-registers/{caixaAberto.Id}/movements");
        var movimentacoes = await resposta.Content.ReadFromJsonAsync<MovimentacaoCaixaResponse[]>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(movimentacoes);
        Assert.NotEmpty(movimentacoes);
    }

    [Fact]
    public async Task cash_operator_can_get_register_summary()
    {
        using var factory = new CaixaApiFactory();
        var caixaAberto = await factory.SemearCaixaAbertoAsync(valorInicial: 25m);
        await factory.SemearVendaProjetadaAsync(caixaAberto.Id, "Dinheiro", 40m);
        await factory.SemearVendaProjetadaAsync(caixaAberto.Id, "Pix", 15m);
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR", CaixaApiFactory.OperadorPadraoId);

        var resposta = await client.GetAsync($"/cash-registers/{caixaAberto.Id}/summary");
        var resumo = await resposta.Content.ReadFromJsonAsync<ResumoCaixaResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(resumo);
        Assert.Equal(2, resumo.QuantidadeVendas);
        Assert.Equal(55m, resumo.TotalVendido);
        Assert.Equal(65m, resumo.DinheiroEsperado);
        Assert.Contains(resumo.TotaisPorFormaPagamento, total =>
            total.FormaPagamento == "Dinheiro" && total.Total == 40m);
    }

    [Fact]
    public async Task cash_summary_without_sales_returns_zero_totals()
    {
        using var factory = new CaixaApiFactory();
        var caixaAberto = await factory.SemearCaixaAbertoAsync(valorInicial: 25m);
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR", CaixaApiFactory.OperadorPadraoId);

        var resposta = await client.GetAsync($"/cash-registers/{caixaAberto.Id}/summary");
        var resumo = await resposta.Content.ReadFromJsonAsync<ResumoCaixaResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(resumo);
        Assert.Equal(0, resumo.QuantidadeVendas);
        Assert.Equal(0m, resumo.TotalVendido);
        Assert.Equal(25m, resumo.DinheiroEsperado);
        Assert.Empty(resumo.TotaisPorFormaPagamento);
    }

    [Fact]
    public async Task cash_summary_rejects_other_operator_register()
    {
        using var factory = new CaixaApiFactory();
        var caixaAberto = await factory.SemearCaixaAbertoAsync(CaixaApiFactory.OutroOperadorId);
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR", CaixaApiFactory.OperadorPadraoId);

        var resposta = await client.GetAsync($"/cash-registers/{caixaAberto.Id}/summary");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task cash_current_requires_authentication()
    {
        using var factory = new CaixaApiFactory();
        var client = factory.CreateClient();

        var resposta = await client.GetAsync("/cash-registers/current");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    private static void Autenticar(HttpClient client, string perfil, Guid operadorId)
    {
        var token = CriarToken(perfil, operadorId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static string CriarToken(string perfil, Guid operadorId)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(CaixaApiFactory.JwtKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var agora = DateTimeOffset.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, operadorId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, $"{perfil.ToLowerInvariant()}@frentecaixa.local"),
            new Claim("role", perfil),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, agora.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            CaixaApiFactory.JwtIssuer,
            CaixaApiFactory.JwtAudience,
            claims,
            agora.UtcDateTime,
            agora.AddMinutes(15).UtcDateTime,
            credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
