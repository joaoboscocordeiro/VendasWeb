namespace FrenteCaixa.BuildingBlocks.Outbox;

public interface IRepositorioOutbox
{
    Task<IReadOnlyCollection<RegistroOutbox>> ObterPendentesAsync(int quantidade, CancellationToken cancellationToken);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
