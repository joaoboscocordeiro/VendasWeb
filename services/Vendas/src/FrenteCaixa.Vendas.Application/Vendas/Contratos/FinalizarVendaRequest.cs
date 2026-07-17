namespace FrenteCaixa.Vendas.Application.Vendas.Contratos;

public sealed record FinalizarVendaRequest(
    string FormaPagamento,
    decimal ValorPago);
