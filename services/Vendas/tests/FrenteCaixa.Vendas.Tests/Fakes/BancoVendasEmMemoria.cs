using FrenteCaixa.Vendas.Domain.Vendas;

namespace FrenteCaixa.Vendas.Tests;

public sealed class BancoVendasEmMemoria
{
    public List<Venda> Vendas { get; } = [];
    public List<PagamentoFake> Pagamentos { get; } = [];
    public List<DeducaoEstoqueFake> DeducoesEstoque { get; } = [];
    public List<string> OutboxRoutingKeys { get; } = [];

    public Guid CaixaAtualId { get; set; } = VendasApiFactory.CaixaPadraoId;
    public Guid CaixaAtualOperadorId { get; set; } = VendasApiFactory.OperadorPadraoId;
    public string CaixaAtualStatus { get; set; } = "Aberto";
    public bool RejeitarDeducaoEstoque { get; set; }
    public bool RejeitarPagamento { get; set; }

    public void Limpar()
    {
        Vendas.Clear();
        Pagamentos.Clear();
        DeducoesEstoque.Clear();
        OutboxRoutingKeys.Clear();
        CaixaAtualId = VendasApiFactory.CaixaPadraoId;
        CaixaAtualOperadorId = VendasApiFactory.OperadorPadraoId;
        CaixaAtualStatus = "Aberto";
        RejeitarDeducaoEstoque = false;
        RejeitarPagamento = false;
    }
}

public sealed record PagamentoFake(
    Guid Id,
    Guid VendaId,
    string FormaPagamento,
    decimal ValorVenda,
    decimal ValorPago);

public sealed record DeducaoEstoqueFake(
    Guid VendaId,
    Guid ProdutoId,
    decimal Quantidade);
