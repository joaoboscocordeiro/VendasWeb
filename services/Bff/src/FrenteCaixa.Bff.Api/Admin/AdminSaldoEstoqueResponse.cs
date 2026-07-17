namespace FrenteCaixa.Bff.Api.Admin;

public sealed record AdminSaldoEstoqueResponse(
    Guid ProdutoId,
    decimal QuantidadeDisponivel,
    DateTimeOffset AtualizadoEm);
