namespace FrenteCaixa.Bff.Api.Admin;

public sealed record AdminMovimentacaoEstoqueResponse(
    Guid Id,
    Guid ProdutoId,
    string Tipo,
    decimal Quantidade,
    decimal QuantidadeAnterior,
    decimal QuantidadeAtual,
    string Motivo,
    DateTimeOffset CriadaEm);
