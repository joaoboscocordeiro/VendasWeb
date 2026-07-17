namespace FrenteCaixa.Pagamentos.Application.Pagamentos.Contratos;

public sealed record RegistrarPagamentoRequest(
    Guid VendaId,
    string FormaPagamento,
    decimal ValorVenda,
    decimal ValorPago);
