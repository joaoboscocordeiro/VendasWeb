namespace FrenteCaixa.Vendas.Application.Vendas.Contratos;

public sealed record ItemVendaResponse(
    Guid Id,
    Guid ProdutoId,
    string DescricaoProduto,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal,
    DateTimeOffset CriadoEm);
