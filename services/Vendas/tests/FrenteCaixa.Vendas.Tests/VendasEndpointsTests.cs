using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Vendas.Application.Vendas.Contratos;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Vendas.Tests;

public sealed class VendasEndpointsTests
{
    [Fact]
    public async Task sales_operator_can_start_sale()
    {
        using var factory = new VendasApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, VendasApiFactory.OperadorPadraoId, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            "/sales",
            new IniciarVendaRequest(VendasApiFactory.CaixaPadraoId));
        var venda = await resposta.Content.ReadFromJsonAsync<VendaResponse>();

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(venda);
        Assert.Equal("EmAndamento", venda.Status);
        Assert.Empty(venda.Itens);
        Assert.Equal(0, venda.Total);
    }

    [Fact]
    public async Task sales_operator_can_add_item_and_total_is_recalculated()
    {
        using var factory = new VendasApiFactory();
        var vendaExistente = await factory.SemearVendaAsync();
        var client = factory.CreateClient();
        Autenticar(client, VendasApiFactory.OperadorPadraoId, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            $"/sales/{vendaExistente.Id}/items",
            new AdicionarItemVendaRequest(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "Cafe Torrado 500g",
                2,
                7.50m));
        var venda = await resposta.Content.ReadFromJsonAsync<VendaResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(venda);
        var item = Assert.Single(venda.Itens);
        Assert.Equal(15.00m, item.Subtotal);
        Assert.Equal(15.00m, venda.Total);
    }

    [Fact]
    public async Task sales_add_item_rejects_invalid_quantity()
    {
        using var factory = new VendasApiFactory();
        var vendaExistente = await factory.SemearVendaAsync();
        var client = factory.CreateClient();
        Autenticar(client, VendasApiFactory.OperadorPadraoId, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            $"/sales/{vendaExistente.Id}/items",
            new AdicionarItemVendaRequest(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "Cafe Torrado 500g",
                0,
                7.50m));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task sales_operator_can_remove_item_and_total_is_recalculated()
    {
        using var factory = new VendasApiFactory();
        var vendaExistente = await factory.SemearVendaComItensAsync();
        var itemRemovido = vendaExistente.Itens.First();
        var client = factory.CreateClient();
        Autenticar(client, VendasApiFactory.OperadorPadraoId, "VENDEDOR");

        var resposta = await client.DeleteAsync($"/sales/{vendaExistente.Id}/items/{itemRemovido.Id}");
        var venda = await resposta.Content.ReadFromJsonAsync<VendaResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(venda);
        Assert.DoesNotContain(venda.Itens, item => item.Id == itemRemovido.Id);
        Assert.Single(venda.Itens);
        Assert.Equal(5.90m, venda.Total);
    }

    [Fact]
    public async Task sales_operator_cannot_get_other_operator_sale()
    {
        using var factory = new VendasApiFactory();
        var vendaExistente = await factory.SemearVendaAsync(VendasApiFactory.OutroOperadorId);
        var client = factory.CreateClient();
        Autenticar(client, VendasApiFactory.OperadorPadraoId, "VENDEDOR");

        var resposta = await client.GetAsync($"/sales/{vendaExistente.Id}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    private static void Autenticar(HttpClient client, Guid operadorId, string perfil)
    {
        var token = CriarToken(operadorId, perfil);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static string CriarToken(Guid operadorId, string perfil)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(VendasApiFactory.JwtKey));
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
            VendasApiFactory.JwtIssuer,
            VendasApiFactory.JwtAudience,
            claims,
            agora.UtcDateTime,
            agora.AddMinutes(15).UtcDateTime,
            credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
