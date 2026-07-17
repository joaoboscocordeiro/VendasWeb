namespace FrenteCaixa.Bff.Api.Pdv;

public sealed record PdvResumoCaixaResponse(
    Guid CaixaId,
    Guid OperadorId,
    string Status,
    decimal ValorInicial,
    decimal? ValorFechamento,
    int QuantidadeVendas,
    decimal TotalVendido,
    decimal DinheiroEsperado,
    decimal? DiferencaPrevista,
    IReadOnlyCollection<PdvTotalFormaPagamentoCaixaResponse> TotaisPorFormaPagamento);

public sealed record PdvTotalFormaPagamentoCaixaResponse(
    string FormaPagamento,
    int QuantidadeVendas,
    decimal Total);
