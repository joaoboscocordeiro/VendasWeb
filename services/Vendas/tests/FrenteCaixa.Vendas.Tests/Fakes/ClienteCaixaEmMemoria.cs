using FrenteCaixa.Vendas.Application.Vendas.Integracoes;

namespace FrenteCaixa.Vendas.Tests;

public sealed class ClienteCaixaEmMemoria : IClienteCaixa
{
    private readonly BancoVendasEmMemoria _banco;

    public ClienteCaixaEmMemoria(BancoVendasEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<ResultadoIntegracao<CaixaAtualIntegracao>> ObterCaixaAtualAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(ResultadoIntegracao<CaixaAtualIntegracao>.Ok(
            new CaixaAtualIntegracao(
                _banco.CaixaAtualId,
                _banco.CaixaAtualOperadorId,
                _banco.CaixaAtualStatus)));
    }
}
