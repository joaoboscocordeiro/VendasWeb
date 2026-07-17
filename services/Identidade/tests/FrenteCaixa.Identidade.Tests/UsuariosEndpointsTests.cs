using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FrenteCaixa.Identidade.Application.Autenticacao.Contratos;
using FrenteCaixa.Identidade.Application.Usuarios.Contratos;

namespace FrenteCaixa.Identidade.Tests;

public sealed class UsuariosEndpointsTests
{
    [Fact]
    public async Task identity_admin_can_create_user()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();
        await AutenticarAsync(client, "admin@frentecaixa.local");

        var resposta = await client.PostAsJsonAsync(
            "/users",
            new CadastrarUsuarioRequest("Novo Vendedor", "novo.vendedor@frentecaixa.local", "Senha@123", "VENDEDOR"));
        var conteudo = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Contains("novo.vendedor@frentecaixa.local", conteudo);
        Assert.DoesNotContain("senhaHash", conteudo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task identity_create_user_rejects_duplicate_email()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();
        await AutenticarAsync(client, "admin@frentecaixa.local");

        var resposta = await client.PostAsJsonAsync(
            "/users",
            new CadastrarUsuarioRequest("Vendedor Duplicado", "vendedor@frentecaixa.local", "Senha@123", "VENDEDOR"));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task identity_seller_cannot_create_user()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();
        await AutenticarAsync(client, "vendedor@frentecaixa.local");

        var resposta = await client.PostAsJsonAsync(
            "/users",
            new CadastrarUsuarioRequest("Novo Usuario", "sem.permissao@frentecaixa.local", "Senha@123", "VENDEDOR"));

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task identity_admin_can_list_users_without_password_hash()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();
        await AutenticarAsync(client, "admin@frentecaixa.local");

        var resposta = await client.GetAsync("/users");
        var conteudo = await resposta.Content.ReadAsStringAsync();
        var usuarios = await resposta.Content.ReadFromJsonAsync<UsuarioResponse[]>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(usuarios);
        Assert.NotEmpty(usuarios);
        Assert.DoesNotContain("senhaHash", conteudo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task identity_admin_can_update_user()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();
        await AutenticarAsync(client, "admin@frentecaixa.local");
        var vendedor = await ObterUsuarioPorEmailAsync(client, "vendedor@frentecaixa.local");

        var resposta = await client.PutAsJsonAsync(
            $"/users/{vendedor.Id}",
            new AtualizarUsuarioRequest("Vendedor Promovido", "vendedor.promovido@frentecaixa.local", "ADM"));
        var usuarioAtualizado = await resposta.Content.ReadFromJsonAsync<UsuarioResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(usuarioAtualizado);
        Assert.Equal("ADM", usuarioAtualizado.Perfil);
        Assert.Equal("vendedor.promovido@frentecaixa.local", usuarioAtualizado.Email);
    }

    [Fact]
    public async Task identity_update_missing_user_returns_not_found()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();
        await AutenticarAsync(client, "admin@frentecaixa.local");

        var resposta = await client.PutAsJsonAsync(
            $"/users/{Guid.NewGuid()}",
            new AtualizarUsuarioRequest("Usuario Ausente", "ausente@frentecaixa.local", "VENDEDOR"));

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task identity_disabled_user_cannot_login_after_admin_disable()
    {
        using var factory = new IdentidadeApiFactory();
        await factory.SemearUsuariosAsync();
        var client = factory.CreateClient();
        await AutenticarAsync(client, "admin@frentecaixa.local");
        var vendedor = await ObterUsuarioPorEmailAsync(client, "vendedor@frentecaixa.local");

        var inativacao = await client.PatchAsync($"/users/{vendedor.Id}/disable", null);

        client.DefaultRequestHeaders.Authorization = null;
        var login = await client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest("vendedor@frentecaixa.local", "Senha@123"));

        Assert.Equal(HttpStatusCode.NoContent, inativacao.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    private static async Task AutenticarAsync(HttpClient client, string email)
    {
        var resposta = await client.PostAsJsonAsync("/auth/login", new LoginRequest(email, "Senha@123"));
        resposta.EnsureSuccessStatusCode();
        var autenticacao = await resposta.Content.ReadFromJsonAsync<AutenticacaoResponse>()
            ?? throw new InvalidOperationException("Resposta de autenticacao vazia.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.AccessToken);
    }

    private static async Task<UsuarioResponse> ObterUsuarioPorEmailAsync(HttpClient client, string email)
    {
        var resposta = await client.GetAsync("/users");
        resposta.EnsureSuccessStatusCode();

        var usuarios = await resposta.Content.ReadFromJsonAsync<UsuarioResponse[]>()
            ?? throw new InvalidOperationException("Lista de usuarios vazia.");

        return usuarios.Single(usuario => usuario.Email == email);
    }
}
