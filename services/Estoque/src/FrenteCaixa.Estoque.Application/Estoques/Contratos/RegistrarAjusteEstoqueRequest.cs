namespace FrenteCaixa.Estoque.Application.Estoques.Contratos;

public sealed record RegistrarAjusteEstoqueRequest(
    Guid ProdutoId,
    string Tipo,
    decimal Quantidade,
    string Motivo);
