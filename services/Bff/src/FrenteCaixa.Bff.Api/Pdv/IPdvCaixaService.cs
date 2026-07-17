namespace FrenteCaixa.Bff.Api.Pdv;

public interface IPdvCaixaService
{
    Task<PdvCaixaResponse?> ObterAtualAsync(
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task<PdvCaixaResponse> AbrirAsync(
        AbrirCaixaPdvRequest request,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task<PdvCaixaResponse> FecharAsync(
        Guid caixaId,
        FecharCaixaPdvRequest request,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PdvMovimentacaoCaixaResponse>> ListarMovimentacoesAsync(
        Guid caixaId,
        string authorizationHeader,
        CancellationToken cancellationToken);
}
