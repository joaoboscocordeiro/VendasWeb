using System.Net.Http.Json;
using FrenteCaixa.Vendas.Application.Vendas.Integracoes;

namespace FrenteCaixa.Vendas.Infrastructure.Integracoes;

public sealed class CaixaHttpClient : ClienteHttpBase, IClienteCaixa
{
    private readonly HttpClient _httpClient;

    public CaixaHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoIntegracao<CaixaAtualIntegracao>> ObterCaixaAtualAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        try
        {
            AplicarBearer(_httpClient, accessToken);
            var resposta = await _httpClient.GetAsync("/cash-registers/current", cancellationToken);

            return await MapearRespostaAsync(
                resposta,
                async () =>
                {
                    var caixa = await resposta.Content.ReadFromJsonAsync<CaixaAtualResponse>(
                        cancellationToken: cancellationToken);

                    return new CaixaAtualIntegracao(
                        caixa!.Id,
                        caixa.OperadorId,
                        caixa.Status);
                },
                "Nao foi possivel validar o caixa aberto.",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return FalhaIndisponivel<CaixaAtualIntegracao>("Servico de Caixa indisponivel.");
        }
    }

    private sealed record CaixaAtualResponse(
        Guid Id,
        Guid OperadorId,
        string Status);
}
