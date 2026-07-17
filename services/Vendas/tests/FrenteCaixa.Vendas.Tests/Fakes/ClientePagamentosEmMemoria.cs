using FrenteCaixa.Vendas.Application.Vendas.Integracoes;

namespace FrenteCaixa.Vendas.Tests;

public sealed class ClientePagamentosEmMemoria : IClientePagamentos
{
    private readonly BancoVendasEmMemoria _banco;

    public ClientePagamentosEmMemoria(BancoVendasEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<ResultadoIntegracao<PagamentoRegistradoIntegracao>> RegistrarPagamentoAsync(
        Guid vendaId,
        string formaPagamento,
        decimal valorVenda,
        decimal valorPago,
        string accessToken,
        CancellationToken cancellationToken)
    {
        if (_banco.RejeitarPagamento)
        {
            return Task.FromResult(ResultadoIntegracao<PagamentoRegistradoIntegracao>.Falha(
                CodigoErroIntegracao.Conflito,
                "Pagamento rejeitado."));
        }

        var pagamento = new PagamentoFake(Guid.NewGuid(), vendaId, formaPagamento, valorVenda, valorPago);
        _banco.Pagamentos.Add(pagamento);

        return Task.FromResult(ResultadoIntegracao<PagamentoRegistradoIntegracao>.Ok(
            new PagamentoRegistradoIntegracao(pagamento.Id, pagamento.FormaPagamento)));
    }
}
