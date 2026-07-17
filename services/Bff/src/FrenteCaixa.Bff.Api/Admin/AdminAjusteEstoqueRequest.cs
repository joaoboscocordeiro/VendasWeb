namespace FrenteCaixa.Bff.Api.Admin;

public sealed record AdminAjusteEstoqueRequest(
    Guid ProdutoId,
    string Tipo,
    decimal Quantidade,
    string Motivo);
