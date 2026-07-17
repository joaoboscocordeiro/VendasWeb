using FrenteCaixa.Bff.Api.Pdv;

namespace FrenteCaixa.Bff.Tests;

public sealed class FakePdvCaixaService : IPdvCaixaService
{
    public PdvCaixaResponse? CaixaAtual { get; set; } = new(
        Guid.Parse("a74f66b6-e812-47c8-a661-7787c069d38d"),
        Guid.Parse("1f2dfaa8-49bf-49ad-8961-cd613d75a5b8"),
        25m,
        null,
        "Aberto",
        DateTimeOffset.Parse("2026-07-17T08:00:00-03:00"),
        null,
        DateTimeOffset.Parse("2026-07-17T08:00:00-03:00"));

    public string? UltimoAuthorizationHeader { get; private set; }

    public decimal? UltimoValorInicial { get; private set; }

    public decimal? UltimoValorFechamento { get; private set; }

    public Guid? UltimoCaixaId { get; private set; }

    public List<PdvMovimentacaoCaixaResponse> Movimentacoes { get; } =
    [
        new(
            Guid.Parse("407fd57a-579c-4dda-b239-19e6fe415001"),
            Guid.Parse("a74f66b6-e812-47c8-a661-7787c069d38d"),
            Guid.Parse("1f2dfaa8-49bf-49ad-8961-cd613d75a5b8"),
            "Abertura",
            25m,
            "Abertura de caixa",
            DateTimeOffset.Parse("2026-07-17T08:00:00-03:00"))
    ];

    public PdvResumoCaixaResponse Resumo { get; set; } = new(
        Guid.Parse("a74f66b6-e812-47c8-a661-7787c069d38d"),
        Guid.Parse("1f2dfaa8-49bf-49ad-8961-cd613d75a5b8"),
        "Aberto",
        25m,
        null,
        2,
        55m,
        65m,
        null,
        [
            new("Dinheiro", 1, 40m),
            new("Pix", 1, 15m)
        ]);

    public Task<PdvCaixaResponse?> ObterAtualAsync(
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        UltimoAuthorizationHeader = authorizationHeader;

        return Task.FromResult(CaixaAtual);
    }

    public Task<PdvCaixaResponse> AbrirAsync(
        AbrirCaixaPdvRequest request,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        UltimoAuthorizationHeader = authorizationHeader;
        UltimoValorInicial = request.ValorInicial;
        CaixaAtual = new PdvCaixaResponse(
            Guid.Parse("d50d0d24-6a7d-4303-a4e3-cdb43b66c6c1"),
            Guid.Parse("1f2dfaa8-49bf-49ad-8961-cd613d75a5b8"),
            request.ValorInicial,
            null,
            "Aberto",
            DateTimeOffset.Parse("2026-07-17T09:00:00-03:00"),
            null,
            DateTimeOffset.Parse("2026-07-17T09:00:00-03:00"));

        return Task.FromResult(CaixaAtual);
    }

    public Task<PdvCaixaResponse> FecharAsync(
        Guid caixaId,
        FecharCaixaPdvRequest request,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        UltimoAuthorizationHeader = authorizationHeader;
        UltimoValorFechamento = request.ValorFechamento;
        UltimoCaixaId = caixaId;
        CaixaAtual = new PdvCaixaResponse(
            caixaId,
            Guid.Parse("1f2dfaa8-49bf-49ad-8961-cd613d75a5b8"),
            CaixaAtual?.ValorInicial ?? 25m,
            request.ValorFechamento,
            "Fechado",
            CaixaAtual?.AbertoEm ?? DateTimeOffset.Parse("2026-07-17T08:00:00-03:00"),
            DateTimeOffset.Parse("2026-07-17T10:00:00-03:00"),
            DateTimeOffset.Parse("2026-07-17T10:00:00-03:00"));

        return Task.FromResult(CaixaAtual);
    }

    public Task<IReadOnlyCollection<PdvMovimentacaoCaixaResponse>> ListarMovimentacoesAsync(
        Guid caixaId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        UltimoAuthorizationHeader = authorizationHeader;
        UltimoCaixaId = caixaId;

        IReadOnlyCollection<PdvMovimentacaoCaixaResponse> movimentacoes = Movimentacoes
            .Where(movimentacao => movimentacao.CaixaId == caixaId)
            .ToArray();

        return Task.FromResult(movimentacoes);
    }

    public Task<PdvResumoCaixaResponse> ObterResumoAsync(
        Guid caixaId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        UltimoAuthorizationHeader = authorizationHeader;
        UltimoCaixaId = caixaId;

        return Task.FromResult(Resumo with { CaixaId = caixaId });
    }
}
