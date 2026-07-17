using FrenteCaixa.Caixa.Application.Caixas.Contratos;

namespace FrenteCaixa.Caixa.Application.Caixas.Interfaces;

public interface IServicoCaixa
{
    Task<ResultadoOperacao<CaixaOperacionalResponse>> AbrirAsync(
        Guid operadorId,
        AbrirCaixaRequest request,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<CaixaOperacionalResponse>> ObterAtualAsync(
        Guid operadorId,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<CaixaOperacionalResponse>> FecharAsync(
        Guid operadorId,
        Guid caixaId,
        FecharCaixaRequest request,
        CancellationToken cancellationToken);

    Task<ResultadoOperacao<IReadOnlyCollection<MovimentacaoCaixaResponse>>> ListarMovimentacoesAsync(
        Guid operadorId,
        Guid caixaId,
        CancellationToken cancellationToken);
}
