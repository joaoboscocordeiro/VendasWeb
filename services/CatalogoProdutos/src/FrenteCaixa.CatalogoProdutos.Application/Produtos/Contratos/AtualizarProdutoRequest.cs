namespace FrenteCaixa.CatalogoProdutos.Application.Produtos.Contratos;

public sealed record AtualizarProdutoRequest(
    string Descricao,
    string? CodigoBarrasEan,
    decimal PrecoCusto,
    decimal PrecoVenda);
