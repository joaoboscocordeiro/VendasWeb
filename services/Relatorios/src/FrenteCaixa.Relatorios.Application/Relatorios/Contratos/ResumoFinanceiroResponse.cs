namespace FrenteCaixa.Relatorios.Application.Relatorios.Contratos;

public sealed record ResumoFinanceiroResponse(
    int QuantidadeVendas,
    decimal ValorTotal,
    IReadOnlyCollection<TotalPorFormaPagamentoResponse> TotaisPorFormaPagamento);
