using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Estoque.Application.Estoques.Contratos;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Estoque.Tests;

public sealed class EstoqueEndpointsTests
{
    [Fact]
    public async Task stock_admin_can_register_entry_adjustment()
    {
        using var factory = new EstoqueApiFactory();
        var produtoId = Guid.NewGuid();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.PostAsJsonAsync(
            "/stock/adjustments",
            new RegistrarAjusteEstoqueRequest(produtoId, "Entrada", 10m, "Entrada inicial"));
        var movimentacao = await resposta.Content.ReadFromJsonAsync<MovimentacaoEstoqueResponse>();
        var consulta = await client.GetAsync($"/stock/products/{produtoId}");
        var saldo = await consulta.Content.ReadFromJsonAsync<SaldoProdutoResponse>();

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(movimentacao);
        Assert.Equal(10m, movimentacao.QuantidadeAtual);
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        Assert.NotNull(saldo);
        Assert.Equal(10m, saldo.QuantidadeDisponivel);
    }

    [Fact]
    public async Task stock_seller_cannot_register_adjustment()
    {
        using var factory = new EstoqueApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            "/stock/adjustments",
            new RegistrarAjusteEstoqueRequest(Guid.NewGuid(), "Entrada", 10m, "Entrada restrita"));

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task stock_adjustment_rejects_invalid_quantity()
    {
        using var factory = new EstoqueApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.PostAsJsonAsync(
            "/stock/adjustments",
            new RegistrarAjusteEstoqueRequest(Guid.NewGuid(), "Entrada", 0m, "Quantidade invalida"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task stock_output_rejects_negative_stock()
    {
        using var factory = new EstoqueApiFactory();
        var produtoId = Guid.NewGuid();
        await factory.SemearSaldoAsync(produtoId, 3m);
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.PostAsJsonAsync(
            "/stock/adjustments",
            new RegistrarAjusteEstoqueRequest(produtoId, "Saida", 5m, "Quebra operacional"));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task stock_authenticated_user_can_get_balance()
    {
        using var factory = new EstoqueApiFactory();
        var produtoId = Guid.NewGuid();
        await factory.SemearSaldoAsync(produtoId, 7m);
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync($"/stock/products/{produtoId}");
        var saldo = await resposta.Content.ReadFromJsonAsync<SaldoProdutoResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(saldo);
        Assert.Equal(7m, saldo.QuantidadeDisponivel);
    }

    [Fact]
    public async Task stock_authenticated_user_can_list_movements()
    {
        using var factory = new EstoqueApiFactory();
        var produtoId = Guid.NewGuid();
        await factory.SemearSaldoAsync(produtoId, 4m);
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync($"/stock/products/{produtoId}/movements");
        var movimentacoes = await resposta.Content.ReadFromJsonAsync<MovimentacaoEstoqueResponse[]>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(movimentacoes);
        Assert.NotEmpty(movimentacoes);
    }

    [Fact]
    public async Task stock_balance_requires_authentication()
    {
        using var factory = new EstoqueApiFactory();
        var client = factory.CreateClient();

        var resposta = await client.GetAsync($"/stock/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    private static void Autenticar(HttpClient client, string perfil)
    {
        var token = CriarToken(perfil);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static string CriarToken(string perfil)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(EstoqueApiFactory.JwtKey));
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
            EstoqueApiFactory.JwtIssuer,
            EstoqueApiFactory.JwtAudience,
            claims,
            agora.UtcDateTime,
            agora.AddMinutes(15).UtcDateTime,
            credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
