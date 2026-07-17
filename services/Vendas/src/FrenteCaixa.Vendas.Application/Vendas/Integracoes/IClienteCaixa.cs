namespace FrenteCaixa.Vendas.Application.Vendas.Integracoes;

public interface IClienteCaixa
{
    Task<ResultadoIntegracao<CaixaAtualIntegracao>> ObterCaixaAtualAsync(
        string accessToken,
        CancellationToken cancellationToken);
}
