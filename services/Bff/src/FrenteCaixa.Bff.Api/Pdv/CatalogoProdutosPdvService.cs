using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace FrenteCaixa.Bff.Api.Pdv;

public sealed class CatalogoProdutosPdvService : IPdvProdutosService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly PdvBootstrapOptions _options;

    public CatalogoProdutosPdvService(
        HttpClient httpClient,
        IOptions<PdvBootstrapOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyCollection<PdvProdutoResponse>> BuscarAsync(
        string? termo,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        var caminho = "/products?onlyActive=true";

        if (!string.IsNullOrWhiteSpace(termo))
        {
            caminho += $"&term={Uri.EscapeDataString(termo.Trim())}";
        }

        using var request = CriarRequest(HttpMethod.Get, caminho, authorizationHeader);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var produtos = await response.Content.ReadFromJsonAsync<CatalogoProdutoResponse[]>(JsonOptions, cancellationToken)
            ?? [];

        return produtos
            .Where(produto => produto.Ativo)
            .Select(Mapear)
            .ToArray();
    }

    public async Task<PdvProdutoResponse?> ObterPorCodigoBarrasAsync(
        string codigoBarrasEan,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(
            HttpMethod.Get,
            $"/products/by-barcode/{Uri.EscapeDataString(codigoBarrasEan.Trim())}",
            authorizationHeader);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var produto = await response.Content.ReadFromJsonAsync<CatalogoProdutoResponse>(JsonOptions, cancellationToken);

        return produto is not null && produto.Ativo
            ? Mapear(produto)
            : null;
    }

    private HttpRequestMessage CriarRequest(
        HttpMethod method,
        string caminho,
        string authorizationHeader)
    {
        var request = new HttpRequestMessage(method, new Uri(ObterBaseCatalogo(), caminho));
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        request.Headers.Accept.ParseAdd("application/json");

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

    private static PdvProdutoResponse Mapear(CatalogoProdutoResponse produto)
    {
        return new PdvProdutoResponse(
            produto.Id,
            produto.Descricao,
            produto.CodigoBarrasEan,
            produto.PrecoVenda,
            produto.Ativo);
    }

    private sealed record CatalogoProdutoResponse(
        Guid Id,
        string Descricao,
        string? CodigoBarrasEan,
        decimal PrecoVenda,
        bool Ativo);
}
