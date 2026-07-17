namespace FrenteCaixa.Relatorios.Application.Relatorios.Contratos;

public sealed record TotalPorFormaPagamentoResponse(
    string FormaPagamento,
    int QuantidadeVendas,
    decimal ValorTotal);
