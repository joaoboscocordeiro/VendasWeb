namespace FrenteCaixa.Caixa.Application.Caixas.Contratos;

public sealed record CaixaOperacionalResponse(
    Guid Id,
    Guid OperadorId,
    decimal ValorInicial,
    decimal? ValorFechamento,
    string Status,
    DateTimeOffset AbertoEm,
    DateTimeOffset? FechadoEm,
    DateTimeOffset AtualizadoEm);
