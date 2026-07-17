namespace FrenteCaixa.Estoque.Application.Estoques.Contratos;

public sealed record MovimentacaoEstoqueResponse(
    Guid Id,
    Guid ProdutoId,
    string Tipo,
    decimal Quantidade,
    decimal QuantidadeAnterior,
    decimal QuantidadeAtual,
    string Motivo,
    DateTimeOffset CriadaEm);
