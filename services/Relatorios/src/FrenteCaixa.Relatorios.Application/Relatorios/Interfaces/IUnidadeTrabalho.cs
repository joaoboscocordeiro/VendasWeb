namespace FrenteCaixa.Relatorios.Application.Relatorios.Interfaces;

public interface IUnidadeTrabalho
{
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
