namespace FrenteCaixa.Bff.Api.Admin;

public sealed record AdminItemVendaRelatorioResponse(
    Guid ProdutoId,
    string DescricaoProduto,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);
