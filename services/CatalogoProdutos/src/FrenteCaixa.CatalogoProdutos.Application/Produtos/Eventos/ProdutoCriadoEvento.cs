namespace FrenteCaixa.CatalogoProdutos.Application.Produtos.Eventos;

public sealed record ProdutoCriadoEvento(
    Guid ProdutoId,
    string Descricao,
    string? CodigoBarrasEan,
    DateTimeOffset OcorridoEm);
