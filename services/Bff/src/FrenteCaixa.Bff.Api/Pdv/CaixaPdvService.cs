using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace FrenteCaixa.Bff.Api.Pdv;

public sealed class CaixaPdvService : IPdvCaixaService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly PdvBootstrapOptions _options;

    public CaixaPdvService(
        HttpClient httpClient,
        IOptions<PdvBootstrapOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<PdvCaixaResponse?> ObterAtualAsync(
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(HttpMethod.Get, "/cash-registers/current", authorizationHeader);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await GarantirSucessoAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<PdvCaixaResponse>(JsonOptions, cancellationToken);
    }

    public async Task<PdvCaixaResponse> AbrirAsync(
        AbrirCaixaPdvRequest request,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var httpRequest = CriarRequest(HttpMethod.Post, "/cash-registers/open", authorizationHeader);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await GarantirSucessoAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<PdvCaixaResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Servico de Caixa retornou resposta vazia.");
    }

    public async Task<PdvCaixaResponse> FecharAsync(
        Guid caixaId,
        FecharCaixaPdvRequest request,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var httpRequest = CriarRequest(
            HttpMethod.Post,
            $"/cash-registers/{caixaId}/close",
            authorizationHeader);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await GarantirSucessoAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<PdvCaixaResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Servico de Caixa retornou resposta vazia.");
    }

    public async Task<IReadOnlyCollection<PdvMovimentacaoCaixaResponse>> ListarMovimentacoesAsync(
        Guid caixaId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(
            HttpMethod.Get,
            $"/cash-registers/{caixaId}/movements",
            authorizationHeader);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await GarantirSucessoAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<PdvMovimentacaoCaixaResponse[]>(
                JsonOptions,
                cancellationToken)
            ?? [];
    }

    private HttpRequestMessage CriarRequest(
        HttpMethod method,
        string caminho,
        string authorizationHeader)
    {
        var request = new HttpRequestMessage(method, new Uri(ObterBaseCaixa(), caminho));
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        request.Headers.Accept.ParseAdd("application/json");

        return request;
    }

    private Uri ObterBaseCaixa()
    {
        var servicoCaixa = _options.Servicos.FirstOrDefault(servico =>
            string.Equals(servico.Nome, "Caixa", StringComparison.OrdinalIgnoreCase));

        var baseUrl = servicoCaixa?.BaseUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Configure PdvBootstrap:Servicos para o Caixa.");
        }

        return new Uri(baseUrl, UriKind.Absolute);
    }

    private static async Task GarantirSucessoAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detalhe = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new PdvCaixaHttpException(
            response.StatusCode,
            string.IsNullOrWhiteSpace(detalhe)
                ? "Servico de Caixa retornou erro."
                : detalhe);
    }
}
