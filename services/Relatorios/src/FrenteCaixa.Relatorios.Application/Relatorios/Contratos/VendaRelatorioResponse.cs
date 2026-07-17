namespace FrenteCaixa.Relatorios.Application.Relatorios.Contratos;

public sealed record VendaRelatorioResponse(
    Guid VendaId,
    Guid CaixaId,
    Guid OperadorId,
    Guid PagamentoId,
    string FormaPagamento,
    decimal ValorTotal,
    DateTimeOffset ConcluidaEm,
    IReadOnlyCollection<ItemVendaRelatorioResponse> Itens);
