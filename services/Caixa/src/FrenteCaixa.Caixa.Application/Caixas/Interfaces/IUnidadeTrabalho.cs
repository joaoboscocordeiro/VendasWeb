namespace FrenteCaixa.Caixa.Application.Caixas.Interfaces;

public interface IUnidadeTrabalho
{
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
