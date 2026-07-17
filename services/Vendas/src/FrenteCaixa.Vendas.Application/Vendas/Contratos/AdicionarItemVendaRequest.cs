namespace FrenteCaixa.Vendas.Application.Vendas.Contratos;

public sealed record AdicionarItemVendaRequest(
    Guid ProdutoId,
    string DescricaoProduto,
    decimal Quantidade,
    decimal PrecoUnitario);
