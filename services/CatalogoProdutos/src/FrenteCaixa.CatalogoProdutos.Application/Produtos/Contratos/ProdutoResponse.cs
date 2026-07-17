namespace FrenteCaixa.CatalogoProdutos.Application.Produtos.Contratos;

public sealed record ProdutoResponse(
    Guid Id,
    string Descricao,
    string? CodigoBarrasEan,
    decimal PrecoCusto,
    decimal PrecoVenda,
    bool Ativo,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm);
