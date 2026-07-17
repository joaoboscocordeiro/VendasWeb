namespace FrenteCaixa.Vendas.Application.Vendas.Eventos;

public sealed record VendaConcluidaEvento(
    Guid VendaId,
    Guid CaixaId,
    Guid OperadorId,
    decimal ValorTotal,
    Guid PagamentoId,
    string FormaPagamento,
    IReadOnlyCollection<ItemVendaConcluidaEvento> Itens,
    DateTimeOffset OcorridoEm);

public sealed record ItemVendaConcluidaEvento(
    Guid ProdutoId,
    string DescricaoProduto,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);
