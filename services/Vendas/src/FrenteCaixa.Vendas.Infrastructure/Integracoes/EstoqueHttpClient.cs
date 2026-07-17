using System.Net.Http.Json;
using FrenteCaixa.Vendas.Application.Vendas.Integracoes;
using FrenteCaixa.Vendas.Domain.Vendas;

namespace FrenteCaixa.Vendas.Infrastructure.Integracoes;

public sealed class EstoqueHttpClient : ClienteHttpBase, IClienteEstoque
{
    private readonly HttpClient _httpClient;

    public EstoqueHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoIntegracao<IReadOnlyCollection<Guid>>> DeduzirVendaAsync(
        Guid vendaId,
        IReadOnlyCollection<ItemVenda> itens,
        string accessToken,
        CancellationToken cancellationToken)
    {
        try
        {
            AplicarBearer(_httpClient, accessToken);
            var request = new DeducaoEstoqueRequest(
                vendaId,
                itens.Select(item => new ItemDeducaoEstoqueRequest(item.ProdutoId, item.Quantidade)).ToArray());
            var resposta = await _httpClient.PostAsJsonAsync("/stock/deductions", request, cancellationToken);

            return await MapearRespostaAsync<IReadOnlyCollection<Guid>>(
                resposta,
                async () =>
                {
                    var movimentacoes = await resposta.Content.ReadFromJsonAsync<MovimentacaoEstoqueResponse[]>(
                        cancellationToken: cancellationToken);

                    return movimentacoes!.Select(movimentacao => movimentacao.Id).ToArray();
                },
                "Nao foi possivel deduzir estoque.",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return FalhaIndisponivel<IReadOnlyCollection<Guid>>("Servico de Estoque indisponivel.");
        }
    }

    private sealed record DeducaoEstoqueRequest(
        Guid VendaId,
        IReadOnlyCollection<ItemDeducaoEstoqueRequest> Itens);

    private sealed record ItemDeducaoEstoqueRequest(
        Guid ProdutoId,
        decimal Quantidade);

    private sealed record MovimentacaoEstoqueResponse(Guid Id);
}
