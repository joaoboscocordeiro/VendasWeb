namespace FrenteCaixa.Vendas.Application.Vendas.Interfaces;

public interface IUnidadeTrabalho
{
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
