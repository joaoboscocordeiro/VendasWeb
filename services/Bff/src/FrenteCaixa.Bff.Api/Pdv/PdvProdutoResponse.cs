namespace FrenteCaixa.Bff.Api.Pdv;

public sealed record PdvProdutoResponse(
    Guid Id,
    string Descricao,
    string? CodigoBarrasEan,
    decimal PrecoVenda,
    bool Ativo);
