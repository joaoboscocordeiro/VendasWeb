namespace FrenteCaixa.Pagamentos.Application.Pagamentos.Interfaces;

public interface IUnidadeTrabalho
{
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
