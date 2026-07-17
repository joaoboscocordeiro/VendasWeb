namespace FrenteCaixa.Bff.Api.Admin;

public sealed record AdminVendaRelatorioResponse(
    Guid VendaId,
    Guid CaixaId,
    Guid OperadorId,
    Guid PagamentoId,
    string FormaPagamento,
    decimal ValorTotal,
    DateTimeOffset ConcluidaEm,
    IReadOnlyCollection<AdminItemVendaRelatorioResponse> Itens);
