using FrenteCaixa.Bff.Api.Admin;

namespace FrenteCaixa.Bff.Tests;

public sealed class FakeAdminProdutosService : IAdminProdutosService
{
    private bool _indisponivel;

    public AdminProdutoResponse Produto { get; } = new(
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        "Cafe Torrado 500g",
        "7891000000001",
        12.50m,
        18.90m,
        true,
        DateTimeOffset.Parse("2026-07-17T10:00:00Z"),
        DateTimeOffset.Parse("2026-07-17T10:00:00Z"));

    public string? UltimoAuthorizationHeader { get; private set; }

    public string? UltimoTermo { get; private set; }

    public bool UltimoSomenteAtivos { get; private set; }

    public Guid? ProdutoInativadoId { get; private set; }

    public void MarcarIndisponivel()
    {
        _indisponivel = true;
    }

    public Task<IReadOnlyCollection<AdminProdutoResponse>> ListarAsync(
        string? termo,
        bool somenteAtivos,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        GarantirDisponivel();
        UltimoAuthorizationHeader = authorizationHeader;
        UltimoTermo = termo;
        UltimoSomenteAtivos = somenteAtivos;

        return Task.FromResult<IReadOnlyCollection<AdminProdutoResponse>>([Produto]);
    }

    public Task<AdminProdutoResponse?> ObterPorIdAsync(
        Guid id,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        GarantirDisponivel();
        UltimoAuthorizationHeader = authorizationHeader;

        return Task.FromResult<AdminProdutoResponse?>(id == Produto.Id ? Produto : null);
    }

    public Task<AdminProdutoResponse> CadastrarAsync(
        AdminProdutoRequest produto,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        GarantirDisponivel();
        UltimoAuthorizationHeader = authorizationHeader;

        return Task.FromResult(MapearRequest(Guid.Parse("22222222-2222-2222-2222-222222222222"), produto));
    }

    public Task<AdminProdutoResponse> AtualizarAsync(
        Guid id,
        AdminProdutoRequest produto,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        GarantirDisponivel();
        UltimoAuthorizationHeader = authorizationHeader;

        return Task.FromResult(MapearRequest(id, produto));
    }

    public Task InativarAsync(
        Guid id,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        GarantirDisponivel();
        UltimoAuthorizationHeader = authorizationHeader;
        ProdutoInativadoId = id;

        return Task.CompletedTask;
    }

    private void GarantirDisponivel()
    {
        if (_indisponivel)
        {
            throw new HttpRequestException("CatalogoProdutos indisponivel.");
        }
    }

    private static AdminProdutoResponse MapearRequest(Guid id, AdminProdutoRequest produto)
    {
        return new AdminProdutoResponse(
            id,
            produto.Descricao,
            produto.CodigoBarrasEan,
            produto.PrecoCusto,
            produto.PrecoVenda,
            true,
            DateTimeOffset.Parse("2026-07-17T11:00:00Z"),
            DateTimeOffset.Parse("2026-07-17T11:00:00Z"));
    }
}
