namespace FrenteCaixa.Estoque.Application.Estoques.Interfaces;

public interface IUnidadeTrabalho
{
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
