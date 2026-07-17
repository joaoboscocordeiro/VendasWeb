using FrenteCaixa.Bff.Api.Admin;

namespace FrenteCaixa.Bff.Tests;

public sealed class FakeAdminRelatoriosService : IAdminRelatoriosService
{
    private bool _indisponivel;

    public AdminVendaRelatorioResponse Venda { get; } = new(
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
        Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
        Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
        "Dinheiro",
        35.90m,
        DateTimeOffset.Parse("2026-07-17T12:00:00Z"),
        [
            new AdminItemVendaRelatorioResponse(
                Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                "Cafe Torrado 500g",
                2m,
                10m,
                20m),
            new AdminItemVendaRelatorioResponse(
                Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                "Acucar Cristal 1kg",
                1m,
                15.90m,
                15.90m)
        ]);

    public string? UltimoAuthorizationHeader { get; private set; }

    public void MarcarIndisponivel()
    {
        _indisponivel = true;
    }

    public Task<IReadOnlyCollection<AdminVendaRelatorioResponse>> ListarVendasAsync(
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        if (_indisponivel)
        {
            throw new HttpRequestException("Relatorios indisponivel.");
        }

        UltimoAuthorizationHeader = authorizationHeader;

        return Task.FromResult<IReadOnlyCollection<AdminVendaRelatorioResponse>>([Venda]);
    }

    public Task<AdminResumoFinanceiroResponse> ObterResumoFinanceiroAsync(
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        if (_indisponivel)
        {
            throw new HttpRequestException("Relatorios indisponivel.");
        }

        UltimoAuthorizationHeader = authorizationHeader;

        return Task.FromResult(new AdminResumoFinanceiroResponse(
            1,
            35.90m,
            [
                new AdminTotalPorFormaPagamentoResponse("Dinheiro", 1, 35.90m)
            ]));
    }
}
