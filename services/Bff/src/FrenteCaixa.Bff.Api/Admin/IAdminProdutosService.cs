namespace FrenteCaixa.Bff.Api.Admin;

public interface IAdminProdutosService
{
    Task<IReadOnlyCollection<AdminProdutoResponse>> ListarAsync(
        string? termo,
        bool somenteAtivos,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task<AdminProdutoResponse?> ObterPorIdAsync(
        Guid id,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task<AdminProdutoResponse> CadastrarAsync(
        AdminProdutoRequest produto,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task<AdminProdutoResponse> AtualizarAsync(
        Guid id,
        AdminProdutoRequest produto,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task InativarAsync(
        Guid id,
        string authorizationHeader,
        CancellationToken cancellationToken);
}
