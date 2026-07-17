namespace FrenteCaixa.Bff.Api.Admin;

public sealed record AdminResumoFinanceiroResponse(
    int QuantidadeVendas,
    decimal ValorTotal,
    IReadOnlyCollection<AdminTotalPorFormaPagamentoResponse> TotaisPorFormaPagamento);
