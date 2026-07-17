namespace FrenteCaixa.Bff.Api.Admin;

public sealed record AdminProdutoRequest(
    string Descricao,
    string? CodigoBarrasEan,
    decimal PrecoCusto,
    decimal PrecoVenda);
