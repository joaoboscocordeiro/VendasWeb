using FrenteCaixa.Bff.Api.Pdv;

namespace FrenteCaixa.Bff.Tests;

public sealed class FakeBackendHealthClient : IBackendHealthClient
{
    private readonly HashSet<string> _indisponiveis = new(StringComparer.OrdinalIgnoreCase);

    public void MarcarIndisponivel(string nome)
    {
        _indisponiveis.Add(nome);
    }

    public Task<ServicoBackendStatusResponse> ObterStatusAsync(
        BackendServicoOptions servico,
        CancellationToken cancellationToken)
    {
        var status = _indisponiveis.Contains(servico.Nome)
            ? ServicoBackendStatusResponse.Indisponivel(servico.Nome, servico.BaseUrl, "Falha simulada.")
            : ServicoBackendStatusResponse.Operacional(servico.Nome, servico.BaseUrl, 200);

        return Task.FromResult(status);
    }
}
