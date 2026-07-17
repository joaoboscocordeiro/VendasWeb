namespace FrenteCaixa.Relatorios.Application.Relatorios.Contratos;

public sealed record ItemVendaRelatorioResponse(
    Guid ProdutoId,
    string DescricaoProduto,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);
