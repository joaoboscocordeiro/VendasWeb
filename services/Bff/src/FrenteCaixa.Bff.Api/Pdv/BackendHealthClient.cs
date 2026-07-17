namespace FrenteCaixa.Bff.Api.Pdv;

public sealed class BackendHealthClient : IBackendHealthClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BackendHealthClient> _logger;

    public BackendHealthClient(HttpClient httpClient, ILogger<BackendHealthClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ServicoBackendStatusResponse> ObterStatusAsync(
        BackendServicoOptions servico,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(servico.Nome) || string.IsNullOrWhiteSpace(servico.BaseUrl))
        {
            return ServicoBackendStatusResponse.Indisponivel(
                servico.Nome,
                servico.BaseUrl,
                "Servico sem nome ou URL base configurada.");
        }

        try
        {
            var endpoint = CriarUri(servico);
            using var resposta = await _httpClient.GetAsync(endpoint, cancellationToken);

            return resposta.IsSuccessStatusCode
                ? ServicoBackendStatusResponse.Operacional(servico.Nome, servico.BaseUrl, (int)resposta.StatusCode)
                : ServicoBackendStatusResponse.Indisponivel(
                    servico.Nome,
                    servico.BaseUrl,
                    $"Health retornou HTTP {(int)resposta.StatusCode}.",
                    (int)resposta.StatusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Falha ao consultar health do servico {Servico}.", servico.Nome);

            return ServicoBackendStatusResponse.Indisponivel(
                servico.Nome,
                servico.BaseUrl,
                "Health indisponivel.");
        }
    }

    private static Uri CriarUri(BackendServicoOptions servico)
    {
        var baseUrl = servico.BaseUrl.EndsWith("/", StringComparison.Ordinal)
            ? servico.BaseUrl
            : $"{servico.BaseUrl}/";
        var healthPath = string.IsNullOrWhiteSpace(servico.HealthPath)
            ? "api/saude"
            : servico.HealthPath.TrimStart('/');

        return new Uri(new Uri(baseUrl), healthPath);
    }
}
