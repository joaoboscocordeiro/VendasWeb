namespace FrenteCaixa.Bff.Api.Admin;

public interface IAdminEstoqueService
{
    Task<AdminSaldoEstoqueResponse> ObterSaldoAsync(
        Guid produtoId,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AdminMovimentacaoEstoqueResponse>> ListarMovimentacoesAsync(
        Guid produtoId,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task<AdminMovimentacaoEstoqueResponse> RegistrarAjusteAsync(
        AdminAjusteEstoqueRequest ajuste,
        string authorizationHeader,
        CancellationToken cancellationToken);
}
