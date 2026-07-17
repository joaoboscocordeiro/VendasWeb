namespace FrenteCaixa.Bff.Api.Pdv;

public interface IPdvProdutosService
{
    Task<IReadOnlyCollection<PdvProdutoResponse>> BuscarAsync(
        string? termo,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task<PdvProdutoResponse?> ObterPorCodigoBarrasAsync(
        string codigoBarrasEan,
        string authorizationHeader,
        CancellationToken cancellationToken);
}
