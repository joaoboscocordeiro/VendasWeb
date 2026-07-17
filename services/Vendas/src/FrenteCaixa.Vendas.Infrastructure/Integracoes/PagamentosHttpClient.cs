using System.Net.Http.Json;
using FrenteCaixa.Vendas.Application.Vendas.Integracoes;

namespace FrenteCaixa.Vendas.Infrastructure.Integracoes;

public sealed class PagamentosHttpClient : ClienteHttpBase, IClientePagamentos
{
    private readonly HttpClient _httpClient;

    public PagamentosHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoIntegracao<PagamentoRegistradoIntegracao>> RegistrarPagamentoAsync(
        Guid vendaId,
        string formaPagamento,
        decimal valorVenda,
        decimal valorPago,
        string accessToken,
        CancellationToken cancellationToken)
    {
        try
        {
            AplicarBearer(_httpClient, accessToken);
            var request = new RegistrarPagamentoRequest(vendaId, formaPagamento, valorVenda, valorPago);
            var resposta = await _httpClient.PostAsJsonAsync("/payments", request, cancellationToken);

            return await MapearRespostaAsync(
                resposta,
                async () =>
                {
                    var pagamento = await resposta.Content.ReadFromJsonAsync<PagamentoResponse>(
                        cancellationToken: cancellationToken);

                    return new PagamentoRegistradoIntegracao(
                        pagamento!.Id,
                        pagamento.FormaPagamento);
                },
                "Nao foi possivel registrar pagamento.",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return FalhaIndisponivel<PagamentoRegistradoIntegracao>("Servico de Pagamentos indisponivel.");
        }
    }

    private sealed record RegistrarPagamentoRequest(
        Guid VendaId,
        string FormaPagamento,
        decimal ValorVenda,
        decimal ValorPago);

    private sealed record PagamentoResponse(
        Guid Id,
        string FormaPagamento);
}
