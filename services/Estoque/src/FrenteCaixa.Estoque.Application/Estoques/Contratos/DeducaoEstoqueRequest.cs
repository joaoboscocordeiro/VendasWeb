namespace FrenteCaixa.Estoque.Application.Estoques.Contratos;

public sealed record DeducaoEstoqueRequest(
    Guid VendaId,
    IReadOnlyCollection<ItemDeducaoEstoqueRequest> Itens);

public sealed record ItemDeducaoEstoqueRequest(
    Guid ProdutoId,
    decimal Quantidade);
