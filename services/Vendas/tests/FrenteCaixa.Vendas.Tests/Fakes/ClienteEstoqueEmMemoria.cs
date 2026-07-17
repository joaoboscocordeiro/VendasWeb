using FrenteCaixa.Vendas.Application.Vendas.Integracoes;
using FrenteCaixa.Vendas.Domain.Vendas;

namespace FrenteCaixa.Vendas.Tests;

public sealed class ClienteEstoqueEmMemoria : IClienteEstoque
{
    private readonly BancoVendasEmMemoria _banco;

    public ClienteEstoqueEmMemoria(BancoVendasEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<ResultadoIntegracao<IReadOnlyCollection<Guid>>> DeduzirVendaAsync(
        Guid vendaId,
        IReadOnlyCollection<ItemVenda> itens,
        string accessToken,
        CancellationToken cancellationToken)
    {
        if (_banco.RejeitarDeducaoEstoque)
        {
            return Task.FromResult(ResultadoIntegracao<IReadOnlyCollection<Guid>>.Falha(
                CodigoErroIntegracao.Conflito,
                "Estoque insuficiente."));
        }

        foreach (var item in itens)
        {
            _banco.DeducoesEstoque.Add(new DeducaoEstoqueFake(vendaId, item.ProdutoId, item.Quantidade));
        }

        IReadOnlyCollection<Guid> movimentacoes = itens.Select(_ => Guid.NewGuid()).ToArray();

        return Task.FromResult(ResultadoIntegracao<IReadOnlyCollection<Guid>>.Ok(movimentacoes));
    }
}
