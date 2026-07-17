namespace FrenteCaixa.Estoque.Application.Estoques.Contratos;

public sealed record SaldoProdutoResponse(
    Guid ProdutoId,
    decimal QuantidadeDisponivel,
    DateTimeOffset AtualizadoEm);
