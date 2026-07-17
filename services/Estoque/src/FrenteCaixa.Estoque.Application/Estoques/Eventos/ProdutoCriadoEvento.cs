namespace FrenteCaixa.Estoque.Application.Estoques.Eventos;

public sealed record ProdutoCriadoEvento(
    Guid ProdutoId,
    string Descricao,
    string? CodigoBarrasEan,
    DateTimeOffset OcorridoEm);
