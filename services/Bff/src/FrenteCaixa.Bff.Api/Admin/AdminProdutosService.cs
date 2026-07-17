using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using FrenteCaixa.Bff.Api.Pdv;
using Microsoft.Extensions.Options;

namespace FrenteCaixa.Bff.Api.Admin;

public sealed class AdminProdutosService : IAdminProdutosService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly PdvBootstrapOptions _options;

    public AdminProdutosService(
        HttpClient httpClient,
        IOptions<PdvBootstrapOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyCollection<AdminProdutoResponse>> ListarAsync(
        string? termo,
        bool somenteAtivos,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        var caminho = $"/products?onlyActive={somenteAtivos.ToString().ToLowerInvariant()}";

        if (!string.IsNullOrWhiteSpace(termo))
        {
            caminho += $"&term={Uri.EscapeDataString(termo.Trim())}";
        }

        using var request = CriarRequest(HttpMethod.Get, caminho, authorizationHeader);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await GarantirSucessoAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminProdutoResponse[]>(
                JsonOptions,
                cancellationToken)
            ?? [];
    }

    public async Task<AdminProdutoResponse?> ObterPorIdAsync(
        Guid id,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(HttpMethod.Get, $"/products/{id}", authorizationHeader);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await GarantirSucessoAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminProdutoResponse>(
            JsonOptions,
            cancellationToken);
    }

    public async Task<AdminProdutoResponse> CadastrarAsync(
        AdminProdutoRequest produto,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(HttpMethod.Post, "/products", authorizationHeader);
        request.Content = CriarJsonContent(produto);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await GarantirSucessoAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminProdutoResponse>(
                JsonOptions,
                cancellationToken)
            ?? throw new InvalidOperationException("CatalogoProdutos retornou produto vazio.");
    }

    public async Task<AdminProdutoResponse> AtualizarAsync(
        Guid id,
        AdminProdutoRequest produto,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(HttpMethod.Put, $"/products/{id}", authorizationHeader);
        request.Content = CriarJsonContent(produto);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await GarantirSucessoAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminProdutoResponse>(
                JsonOptions,
                cancellationToken)
            ?? throw new InvalidOperationException("CatalogoProdutos retornou produto vazio.");
    }

    public async Task InativarAsync(
        Guid id,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(HttpMethod.Patch, $"/products/{id}/disable", authorizationHeader);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await GarantirSucessoAsync(response, cancellationToken);
    }

    private HttpRequestMessage CriarRequest(
        HttpMethod method,
        string caminho,
        string authorizationHeader)
    {
        var request = new HttpRequestMessage(method, new Uri(ObterBaseCatalogo(), caminho));
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        request.Headers.Accept.ParseAdd(MediaTypeNames.Application.Json);

        return request;
    }

    private Uri ObterBaseCatalogo()
    {
        var servicoCatalogo = _options.Servicos.FirstOrDefault(servico =>
            string.Equals(servico.Nome, "CatalogoProdutos", StringComparison.OrdinalIgnoreCase));

        var baseUrl = servicoCatalogo?.BaseUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Configure PdvBootstrap:Servicos para o CatalogoProdutos.");
        }

        return new Uri(baseUrl, UriKind.Absolute);
    }

    private static StringContent CriarJsonContent(AdminProdutoRequest produto)
    {
        return new StringContent(
            JsonSerializer.Serialize(produto, JsonOptions),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);
    }

    private static async Task GarantirSucessoAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new AdminProdutosHttpException(
            response.StatusCode,
            await LerMensagemErroAsync(response, cancellationToken));
    }

    private static async Task<string> LerMensagemErroAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var conteudo = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(conteudo))
        {
            return "CatalogoProdutos retornou erro.";
        }

        try
        {
            using var json = JsonDocument.Parse(conteudo);
            var raiz = json.RootElement;

            foreach (var propriedade in new[] { "erro", "mensagem", "message", "title", "detail" })
            {
                if (raiz.TryGetProperty(propriedade, out var valor) && valor.ValueKind == JsonValueKind.String)
                {
                    return valor.GetString() ?? "CatalogoProdutos retornou erro.";
                }
            }
        }
        catch (JsonException)
        {
            return conteudo;
        }

        return conteudo;
    }
}
