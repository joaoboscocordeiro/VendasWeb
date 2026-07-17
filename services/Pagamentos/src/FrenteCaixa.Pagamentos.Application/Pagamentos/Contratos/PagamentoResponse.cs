namespace FrenteCaixa.Pagamentos.Application.Pagamentos.Contratos;

public sealed record PagamentoResponse(
    Guid Id,
    Guid VendaId,
    string FormaPagamento,
    decimal ValorVenda,
    decimal ValorPago,
    decimal Troco,
    string Status,
    DateTimeOffset CriadoEm);
