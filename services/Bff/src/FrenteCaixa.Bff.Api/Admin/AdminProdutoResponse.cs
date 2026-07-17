namespace FrenteCaixa.Bff.Api.Admin;

public sealed record AdminProdutoResponse(
    Guid Id,
    string Descricao,
    string? CodigoBarrasEan,
    decimal PrecoCusto,
    decimal PrecoVenda,
    bool Ativo,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm);
