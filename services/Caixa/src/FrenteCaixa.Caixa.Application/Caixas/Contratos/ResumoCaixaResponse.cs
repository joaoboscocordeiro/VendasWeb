namespace FrenteCaixa.Caixa.Application.Caixas.Contratos;

public sealed record ResumoCaixaResponse(
    Guid CaixaId,
    Guid OperadorId,
    string Status,
    decimal ValorInicial,
    decimal? ValorFechamento,
    int QuantidadeVendas,
    decimal TotalVendido,
    decimal DinheiroEsperado,
    decimal? DiferencaPrevista,
    IReadOnlyCollection<TotalFormaPagamentoCaixaResponse> TotaisPorFormaPagamento);

public sealed record TotalFormaPagamentoCaixaResponse(
    string FormaPagamento,
    int QuantidadeVendas,
    decimal Total);
