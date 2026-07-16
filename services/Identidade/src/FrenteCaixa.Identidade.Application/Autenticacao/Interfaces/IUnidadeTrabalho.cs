namespace FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;

public interface IUnidadeTrabalho
{
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
