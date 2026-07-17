namespace FrenteCaixa.CatalogoProdutos.Application.Produtos.Contratos;

public sealed record CadastrarProdutoRequest(
    string Descricao,
    string? CodigoBarrasEan,
    decimal PrecoCusto,
    decimal PrecoVenda);
