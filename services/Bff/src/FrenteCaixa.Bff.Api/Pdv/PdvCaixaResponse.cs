namespace FrenteCaixa.Bff.Api.Pdv;

public sealed record PdvCaixaResponse(
    Guid Id,
    Guid OperadorId,
    decimal ValorInicial,
    decimal? ValorFechamento,
    string Status,
    DateTimeOffset AbertoEm,
    DateTimeOffset? FechadoEm,
    DateTimeOffset AtualizadoEm);
