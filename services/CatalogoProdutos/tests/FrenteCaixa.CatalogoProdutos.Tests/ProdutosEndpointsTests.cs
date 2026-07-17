using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.CatalogoProdutos.Application.Produtos.Contratos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.CatalogoProdutos.Tests;

public sealed class ProdutosEndpointsTests
{
    [Fact]
    public async Task catalog_admin_can_create_product()
    {
        using var factory = new CatalogoApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.PostAsJsonAsync(
            "/products",
            new CadastrarProdutoRequest("Cafe Torrado 500g", "7891234567895", 8.50m, 14.90m));
        var produto = await resposta.Content.ReadFromJsonAsync<ProdutoResponse>();

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(produto);
        Assert.Equal("Cafe Torrado 500g", produto.Descricao);
        Assert.Equal("7891234567895", produto.CodigoBarrasEan);
    }

    [Fact]
    public async Task catalog_create_product_records_product_created_outbox()
    {
        using var factory = new CatalogoApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.PostAsJsonAsync(
            "/products",
            new CadastrarProdutoRequest("Acucar Cristal 1kg", "7891000000002", 3.20m, 5.90m));

        using var scope = factory.Services.CreateScope();
        var banco = scope.ServiceProvider.GetRequiredService<BancoCatalogoEmMemoria>();
        var produtoComEvento = Assert.Single(banco.ProdutosCriadosComEvento);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Equal("Acucar Cristal 1kg", produtoComEvento.Descricao);
        Assert.Equal("7891000000002", produtoComEvento.CodigoBarrasEan);
    }

    [Fact]
    public async Task catalog_seller_cannot_create_product()
    {
        using var factory = new CatalogoApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.PostAsJsonAsync(
            "/products",
            new CadastrarProdutoRequest("Produto Restrito", "7890000000001", 1m, 2m));

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task catalog_create_product_rejects_duplicate_barcode()
    {
        using var factory = new CatalogoApiFactory();
        await factory.SemearProdutoAsync();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.PostAsJsonAsync(
            "/products",
            new CadastrarProdutoRequest("Cafe Duplicado", "7891234567895", 9m, 15m));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task catalog_create_product_rejects_invalid_prices()
    {
        using var factory = new CatalogoApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.PostAsJsonAsync(
            "/products",
            new CadastrarProdutoRequest("Preco Invalido", "7891234567896", 1m, 0m));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task catalog_authenticated_user_can_list_products()
    {
        using var factory = new CatalogoApiFactory();
        await factory.SemearProdutoAsync();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync("/products");
        var produtos = await resposta.Content.ReadFromJsonAsync<ProdutoResponse[]>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(produtos);
        Assert.NotEmpty(produtos);
    }

    [Fact]
    public async Task catalog_admin_can_update_product()
    {
        using var factory = new CatalogoApiFactory();
        var produtoExistente = await factory.SemearProdutoAsync();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.PutAsJsonAsync(
            $"/products/{produtoExistente.Id}",
            new AtualizarProdutoRequest("Cafe Premium 500g", "7891234567895", 10m, 19.90m));
        var produto = await resposta.Content.ReadFromJsonAsync<ProdutoResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(produto);
        Assert.Equal("Cafe Premium 500g", produto.Descricao);
        Assert.Equal(19.90m, produto.PrecoVenda);
    }

    [Fact]
    public async Task catalog_update_missing_product_returns_not_found()
    {
        using var factory = new CatalogoApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.PutAsJsonAsync(
            $"/products/{Guid.NewGuid()}",
            new AtualizarProdutoRequest("Produto Ausente", "7891234567897", 1m, 2m));

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task catalog_admin_can_disable_product()
    {
        using var factory = new CatalogoApiFactory();
        var produtoExistente = await factory.SemearProdutoAsync();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var inativacao = await client.PatchAsync($"/products/{produtoExistente.Id}/disable", null);
        var consulta = await client.GetAsync($"/products/{produtoExistente.Id}");
        var produto = await consulta.Content.ReadFromJsonAsync<ProdutoResponse>();

        Assert.Equal(HttpStatusCode.NoContent, inativacao.StatusCode);
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        Assert.NotNull(produto);
        Assert.False(produto.Ativo);
    }

    private static void Autenticar(HttpClient client, string perfil)
    {
        var token = CriarToken(perfil);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static string CriarToken(string perfil)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(CatalogoApiFactory.JwtKey));
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
            CatalogoApiFactory.JwtIssuer,
            CatalogoApiFactory.JwtAudience,
            claims,
            agora.UtcDateTime,
            agora.AddMinutes(15).UtcDateTime,
            credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
