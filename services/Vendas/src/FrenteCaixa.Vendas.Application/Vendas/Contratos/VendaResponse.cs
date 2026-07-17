namespace FrenteCaixa.Vendas.Application.Vendas.Contratos;

public sealed record VendaResponse(
    Guid Id,
    Guid OperadorId,
    Guid CaixaId,
    string Status,
    decimal Total,
    IReadOnlyCollection<ItemVendaResponse> Itens,
    DateTimeOffset CriadaEm,
    DateTimeOffset AtualizadaEm);
