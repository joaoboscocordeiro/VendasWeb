namespace FrenteCaixa.Vendas.Application.Vendas.Contratos;

public sealed record CheckoutVendaResponse(
    VendaResponse Venda,
    Guid PagamentoId,
    DateTimeOffset ConcluidaEm);
