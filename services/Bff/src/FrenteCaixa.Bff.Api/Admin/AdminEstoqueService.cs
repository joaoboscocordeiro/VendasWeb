using System.Net.Mime;
using System.Text;
using System.Text.Json;
using FrenteCaixa.Bff.Api.Pdv;
using Microsoft.Extensions.Options;

namespace FrenteCaixa.Bff.Api.Admin;

public sealed class AdminEstoqueService : IAdminEstoqueService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly PdvBootstrapOptions _options;

    public AdminEstoqueService(
        HttpClient httpClient,
        IOptions<PdvBootstrapOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<AdminSaldoEstoqueResponse> ObterSaldoAsync(
        Guid produtoId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(HttpMethod.Get, $"/stock/products/{produtoId}", authorizationHeader);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await GarantirSucessoAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminSaldoEstoqueResponse>(
                JsonOptions,
                cancellationToken)
            ?? throw new InvalidOperationException("Estoque retornou saldo vazio.");
    }

    public async Task<IReadOnlyCollection<AdminMovimentacaoEstoqueResponse>> ListarMovimentacoesAsync(
        Guid produtoId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(HttpMethod.Get, $"/stock/products/{produtoId}/movements", authorizationHeader);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await GarantirSucessoAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminMovimentacaoEstoqueResponse[]>(
                JsonOptions,
                cancellationToken)
            ?? [];
    }

    public async Task<AdminMovimentacaoEstoqueResponse> RegistrarAjusteAsync(
        AdminAjusteEstoqueRequest ajuste,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(HttpMethod.Post, "/stock/adjustments", authorizationHeader);
        request.Content = CriarJsonContent(ajuste);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await GarantirSucessoAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminMovimentacaoEstoqueResponse>(
                JsonOptions,
                cancellationToken)
            ?? throw new InvalidOperationException("Estoque retornou movimentacao vazia.");
    }

    private HttpRequestMessage CriarRequest(
        HttpMethod method,
        string caminho,
        string authorizationHeader)
    {
        var request = new HttpRequestMessage(method, new Uri(ObterBaseEstoque(), caminho));
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        request.Headers.Accept.ParseAdd(MediaTypeNames.Application.Json);

        return request;
    }

    private Uri ObterBaseEstoque()
    {
        var servicoEstoque = _options.Servicos.FirstOrDefault(servico =>
            string.Equals(servico.Nome, "Estoque", StringComparison.OrdinalIgnoreCase));

        var baseUrl = servicoEstoque?.BaseUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Configure PdvBootstrap:Servicos para o Estoque.");
        }

        return new Uri(baseUrl, UriKind.Absolute);
    }

    private static StringContent CriarJsonContent(AdminAjusteEstoqueRequest ajuste)
    {
        return new StringContent(
            JsonSerializer.Serialize(ajuste, JsonOptions),
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

        throw new AdminEstoqueHttpException(
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
            return "Estoque retornou erro.";
        }

        try
        {
            using var json = JsonDocument.Parse(conteudo);
            var raiz = json.RootElement;

            foreach (var propriedade in new[] { "erro", "mensagem", "message", "title", "detail" })
            {
                if (raiz.TryGetProperty(propriedade, out var valor) && valor.ValueKind == JsonValueKind.String)
                {
                    return valor.GetString() ?? "Estoque retornou erro.";
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
