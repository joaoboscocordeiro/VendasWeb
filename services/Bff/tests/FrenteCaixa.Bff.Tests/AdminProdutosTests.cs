using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FrenteCaixa.Bff.Api.Admin;
using Microsoft.IdentityModel.Tokens;

namespace FrenteCaixa.Bff.Tests;

public sealed class AdminProdutosTests
{
    [Fact]
    public async Task bff_admin_can_list_products()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.GetAsync("/admin/products?term=cafe&onlyActive=false");
        var produtos = await resposta.Content.ReadFromJsonAsync<AdminProdutoResponse[]>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(produtos);
        var produto = Assert.Single(produtos);
        Assert.Equal(factory.AdminProdutosService.Produto.Id, produto.Id);
        Assert.Equal("Cafe Torrado 500g", produto.Descricao);
        Assert.Equal("cafe", factory.AdminProdutosService.UltimoTermo);
        Assert.False(factory.AdminProdutosService.UltimoSomenteAtivos);
        Assert.StartsWith("Bearer ", factory.AdminProdutosService.UltimoAuthorizationHeader);
    }

    [Fact]
    public async Task bff_admin_can_create_and_update_product()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");
        var cadastro = new AdminProdutoRequest(
            "Bolo de Cenoura",
            "7891000000002",
            4.25m,
            8.50m);

        var respostaCadastro = await client.PostAsJsonAsync("/admin/products", cadastro);
        var produtoCriado = await respostaCadastro.Content.ReadFromJsonAsync<AdminProdutoResponse>();
        Assert.Equal(HttpStatusCode.Created, respostaCadastro.StatusCode);
        Assert.NotNull(produtoCriado);
        Assert.Equal(cadastro.Descricao, produtoCriado.Descricao);
        Assert.Equal(cadastro.PrecoVenda, produtoCriado.PrecoVenda);

        var atualizacao = new AdminProdutoRequest(
            "Bolo de Cenoura Fatia",
            "7891000000003",
            4.50m,
            9.25m);

        var respostaAtualizacao = await client.PutAsJsonAsync(
            $"/admin/products/{produtoCriado.Id}",
            atualizacao);
        var produtoAtualizado = await respostaAtualizacao.Content.ReadFromJsonAsync<AdminProdutoResponse>();

        Assert.Equal(HttpStatusCode.OK, respostaAtualizacao.StatusCode);
        Assert.NotNull(produtoAtualizado);
        Assert.Equal(produtoCriado.Id, produtoAtualizado.Id);
        Assert.Equal(atualizacao.Descricao, produtoAtualizado.Descricao);
        Assert.Equal(atualizacao.CodigoBarrasEan, produtoAtualizado.CodigoBarrasEan);
        Assert.Equal(atualizacao.PrecoCusto, produtoAtualizado.PrecoCusto);
        Assert.Equal(atualizacao.PrecoVenda, produtoAtualizado.PrecoVenda);
    }

    [Fact]
    public async Task bff_admin_can_disable_product()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.PatchAsync(
            $"/admin/products/{factory.AdminProdutosService.Produto.Id}/disable",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(factory.AdminProdutosService.Produto.Id, factory.AdminProdutosService.ProdutoInativadoId);
        Assert.StartsWith("Bearer ", factory.AdminProdutosService.UltimoAuthorizationHeader);
    }

    [Fact]
    public async Task bff_seller_cannot_access_admin_products()
    {
        using var factory = new BffApiFactory();
        var client = factory.CreateClient();
        Autenticar(client, "VENDEDOR");

        var resposta = await client.GetAsync("/admin/products");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task bff_admin_products_unavailable_returns_bad_gateway()
    {
        using var factory = new BffApiFactory();
        factory.AdminProdutosService.MarcarIndisponivel();
        var client = factory.CreateClient();
        Autenticar(client, "ADM");

        var resposta = await client.GetAsync("/admin/products");

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
