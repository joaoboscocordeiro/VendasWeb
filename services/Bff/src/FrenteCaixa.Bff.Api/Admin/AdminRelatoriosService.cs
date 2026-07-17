using System.Text.Json;
using FrenteCaixa.Bff.Api.Pdv;
using Microsoft.Extensions.Options;

namespace FrenteCaixa.Bff.Api.Admin;

public sealed class AdminRelatoriosService : IAdminRelatoriosService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly PdvBootstrapOptions _options;

    public AdminRelatoriosService(
        HttpClient httpClient,
        IOptions<PdvBootstrapOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyCollection<AdminVendaRelatorioResponse>> ListarVendasAsync(
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(HttpMethod.Get, "/reports/sales", authorizationHeader);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AdminVendaRelatorioResponse[]>(
                JsonOptions,
                cancellationToken)
            ?? [];
    }

    public async Task<AdminResumoFinanceiroResponse> ObterResumoFinanceiroAsync(
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = CriarRequest(HttpMethod.Get, "/reports/financial-summary", authorizationHeader);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AdminResumoFinanceiroResponse>(
                JsonOptions,
                cancellationToken)
            ?? throw new InvalidOperationException("Servico de Relatorios retornou resumo vazio.");
    }

    private HttpRequestMessage CriarRequest(
        HttpMethod method,
        string caminho,
        string authorizationHeader)
    {
        var request = new HttpRequestMessage(method, new Uri(ObterBaseRelatorios(), caminho));
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        request.Headers.Accept.ParseAdd("application/json");

        return request;
    }

    private Uri ObterBaseRelatorios()
    {
        var servicoRelatorios = _options.Servicos.FirstOrDefault(servico =>
            string.Equals(servico.Nome, "Relatorios", StringComparison.OrdinalIgnoreCase));

        var baseUrl = servicoRelatorios?.BaseUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Configure PdvBootstrap:Servicos para o Relatorios.");
        }

        return new Uri(baseUrl, UriKind.Absolute);
    }
}
