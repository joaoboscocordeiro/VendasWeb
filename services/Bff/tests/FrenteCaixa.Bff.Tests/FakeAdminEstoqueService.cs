using FrenteCaixa.Bff.Api.Admin;

namespace FrenteCaixa.Bff.Tests;

public sealed class FakeAdminEstoqueService : IAdminEstoqueService
{
    private bool _indisponivel;

    public Guid ProdutoId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public AdminSaldoEstoqueResponse Saldo { get; } = new(
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        12.5m,
        DateTimeOffset.Parse("2026-07-17T12:00:00Z"));

    public AdminMovimentacaoEstoqueResponse Movimentacao { get; } = new(
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        "Entrada",
        5m,
        7.5m,
        12.5m,
        "Compra inicial",
        DateTimeOffset.Parse("2026-07-17T12:05:00Z"));

    public string? UltimoAuthorizationHeader { get; private set; }

    public AdminAjusteEstoqueRequest? UltimoAjuste { get; private set; }

    public void MarcarIndisponivel()
    {
        _indisponivel = true;
    }

    public Task<AdminSaldoEstoqueResponse> ObterSaldoAsync(
        Guid produtoId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        GarantirDisponivel();
        UltimoAuthorizationHeader = authorizationHeader;

        return Task.FromResult(Saldo with { ProdutoId = produtoId });
    }

    public Task<IReadOnlyCollection<AdminMovimentacaoEstoqueResponse>> ListarMovimentacoesAsync(
        Guid produtoId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        GarantirDisponivel();
        UltimoAuthorizationHeader = authorizationHeader;

        return Task.FromResult<IReadOnlyCollection<AdminMovimentacaoEstoqueResponse>>(
            [Movimentacao with { ProdutoId = produtoId }]);
    }

    public Task<AdminMovimentacaoEstoqueResponse> RegistrarAjusteAsync(
        AdminAjusteEstoqueRequest ajuste,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        GarantirDisponivel();
        UltimoAuthorizationHeader = authorizationHeader;
        UltimoAjuste = ajuste;

        var quantidadeAtual = ajuste.Tipo.Equals("Saida", StringComparison.OrdinalIgnoreCase)
            ? Saldo.QuantidadeDisponivel - ajuste.Quantidade
            : Saldo.QuantidadeDisponivel + ajuste.Quantidade;

        return Task.FromResult(new AdminMovimentacaoEstoqueResponse(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            ajuste.ProdutoId,
            ajuste.Tipo,
            ajuste.Quantidade,
            Saldo.QuantidadeDisponivel,
            quantidadeAtual,
            ajuste.Motivo,
            DateTimeOffset.Parse("2026-07-17T12:10:00Z")));
    }

    private void GarantirDisponivel()
    {
        if (_indisponivel)
        {
            throw new HttpRequestException("Estoque indisponivel.");
        }
    }
}
