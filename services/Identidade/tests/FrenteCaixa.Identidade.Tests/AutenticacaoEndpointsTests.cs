using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FrenteCaixa.Identidade.Application.Autenticacao.Contratos;

namespace FrenteCaixa.Identidade.Tests;

public sealed class AutenticacaoEndpointsTests
{
    [Fact]
    public async Task identity_login_returns_jwt_for_active_user()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync("/auth/login", new LoginRequest("vendedor@frentecaixa.local", "Senha@123"));

        resposta.EnsureSuccessStatusCode();
        var autenticacao = await resposta.Content.ReadFromJsonAsync<AutenticacaoResponse>();

        Assert.NotNull(autenticacao);
        Assert.False(string.IsNullOrWhiteSpace(autenticacao.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(autenticacao.RefreshToken));
        Assert.Equal("VENDEDOR", autenticacao.Usuario.Perfil);
    }

    [Fact]
    public async Task identity_login_rejects_invalid_credentials()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync("/auth/login", new LoginRequest("vendedor@frentecaixa.local", "senha-errada"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task identity_login_rejects_inactive_user()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync("/auth/login", new LoginRequest("inativo@frentecaixa.local", "Senha@123"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task identity_refresh_returns_new_access_token()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();
        var autenticacao = await EntrarAsync(client);

        var resposta = await client.PostAsJsonAsync("/auth/refresh", new RenovarTokenRequest(autenticacao.RefreshToken));

        resposta.EnsureSuccessStatusCode();
        var novaAutenticacao = await resposta.Content.ReadFromJsonAsync<AutenticacaoResponse>();

        Assert.NotNull(novaAutenticacao);
        Assert.False(string.IsNullOrWhiteSpace(novaAutenticacao.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(novaAutenticacao.RefreshToken));
        Assert.NotEqual(autenticacao.RefreshToken, novaAutenticacao.RefreshToken);
    }

    [Fact]
    public async Task identity_refresh_rejects_revoked_token()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();
        var autenticacao = await EntrarAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.AccessToken);

        var logout = await client.PostAsJsonAsync("/auth/logout", new LogoutRequest(autenticacao.RefreshToken));
        logout.EnsureSuccessStatusCode();

        var resposta = await client.PostAsJsonAsync("/auth/refresh", new RenovarTokenRequest(autenticacao.RefreshToken));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task identity_me_requires_authentication()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();

        var resposta = await client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task identity_admin_policy_rejects_seller()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();
        var autenticacao = await EntrarAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.AccessToken);

        var resposta = await client.GetAsync("/administracao/verificacao");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    private static async Task<AutenticacaoResponse> EntrarAsync(HttpClient client)
    {
        var resposta = await client.PostAsJsonAsync("/auth/login", new LoginRequest("vendedor@frentecaixa.local", "Senha@123"));
        resposta.EnsureSuccessStatusCode();

        return await resposta.Content.ReadFromJsonAsync<AutenticacaoResponse>()
            ?? throw new InvalidOperationException("Resposta de autenticacao vazia.");
    }
}
