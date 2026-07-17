using FrenteCaixa.Bff.Api.Pdv;

namespace FrenteCaixa.Bff.Tests;

public sealed class FakePdvProdutosService : IPdvProdutosService
{
    private readonly List<PdvProdutoResponse> _produtos =
    [
        new(
            Guid.Parse("2f2cfcb5-5d4e-4e38-a7dd-bfb99c967401"),
            "Cafe Torrado 500g",
            "7891234567895",
            14.90m,
            true)
    ];

    public string? UltimoAuthorizationHeader { get; private set; }

    public Task<IReadOnlyCollection<PdvProdutoResponse>> BuscarAsync(
        string? termo,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        UltimoAuthorizationHeader = authorizationHeader;

        IReadOnlyCollection<PdvProdutoResponse> produtos = _produtos
            .Where(produto =>
                string.IsNullOrWhiteSpace(termo)
                || produto.Descricao.Contains(termo, StringComparison.OrdinalIgnoreCase)
                || string.Equals(produto.CodigoBarrasEan, termo, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return Task.FromResult(produtos);
    }

    public Task<PdvProdutoResponse?> ObterPorCodigoBarrasAsync(
        string codigoBarrasEan,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        UltimoAuthorizationHeader = authorizationHeader;
        var produto = _produtos.SingleOrDefault(produto => produto.CodigoBarrasEan == codigoBarrasEan);

        return Task.FromResult(produto);
    }
}
