using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Pagamentos.Application.Pagamentos.Contratos;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Pagamentos.Tests;

public sealed class PagamentosEndpointsTests
{
    private static readonly Guid OperadorPadraoId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task payments_operator_can_register_cash_payment_with_change()
    {
        using var factory = new PagamentosApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            "/payments",
            new RegistrarPagamentoRequest(
                PagamentosApiFactory.VendaPadraoId,
                "Dinheiro",
                20.00m,
                50.00m));
        var pagamento = await resposta.Content.ReadFromJsonAsync<PagamentoResponse>();

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(pagamento);
        Assert.Equal("Registrado", pagamento.Status);
        Assert.Equal(30.00m, pagamento.Troco);
    }

    [Fact]
    public async Task payments_operator_can_register_card_payment()
    {
        using var factory = new PagamentosApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            "/payments",
            new RegistrarPagamentoRequest(
                PagamentosApiFactory.VendaPadraoId,
                "Cartao",
                42.90m,
                42.90m));
        var pagamento = await resposta.Content.ReadFromJsonAsync<PagamentoResponse>();

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(pagamento);
        Assert.Equal("Cartao", pagamento.FormaPagamento);
        Assert.Equal(0, pagamento.Troco);
    }

    [Fact]
    public async Task payments_cash_payment_rejects_insufficient_amount()
    {
        using var factory = new PagamentosApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            "/payments",
            new RegistrarPagamentoRequest(
                PagamentosApiFactory.VendaPadraoId,
                "Dinheiro",
                20.00m,
                19.99m));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task payments_card_payment_rejects_divergent_amount()
    {
        using var factory = new PagamentosApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            "/payments",
            new RegistrarPagamentoRequest(
                PagamentosApiFactory.VendaPadraoId,
                "Cartao",
                20.00m,
                25.00m));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task payments_operator_can_list_payments_by_sale()
    {
        using var factory = new PagamentosApiFactory();
        var pagamentoExistente = await factory.SemearPagamentoAsync();
        await factory.SemearPagamentoAsync(PagamentosApiFactory.OutraVendaId);
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync($"/payments/by-sale/{PagamentosApiFactory.VendaPadraoId}");
        var pagamentos = await resposta.Content.ReadFromJsonAsync<PagamentoResponse[]>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(pagamentos);
        var pagamento = Assert.Single(pagamentos);
        Assert.Equal(pagamentoExistente.Id, pagamento.Id);
        Assert.Equal(PagamentosApiFactory.VendaPadraoId, pagamento.VendaId);
    }

    [Fact]
    public async Task payments_requires_authentication()
    {
        using var factory = new PagamentosApiFactory();
        var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync(
            "/payments",
            new RegistrarPagamentoRequest(
                PagamentosApiFactory.VendaPadraoId,
                "Dinheiro",
                20.00m,
                20.00m));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    private static void Autenticar(HttpClient client, string perfil)
    {
        var token = CriarToken(perfil);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static string CriarToken(string perfil)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(PagamentosApiFactory.JwtKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var agora = DateTimeOffset.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, OperadorPadraoId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, $"{perfil.ToLowerInvariant()}@frentecaixa.local"),
            new Claim("role", perfil),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, agora.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            PagamentosApiFactory.JwtIssuer,
            PagamentosApiFactory.JwtAudience,
            claims,
            agora.UtcDateTime,
            agora.AddMinutes(15).UtcDateTime,
            credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
