namespace FrenteCaixa.Bff.Api.Admin;

public sealed record AdminTotalPorFormaPagamentoResponse(
    string FormaPagamento,
    int QuantidadeVendas,
    decimal ValorTotal);
