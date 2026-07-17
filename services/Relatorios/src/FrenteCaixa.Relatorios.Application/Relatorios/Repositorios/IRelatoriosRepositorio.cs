using FrenteCaixa.Relatorios.Domain.Relatorios;

namespace FrenteCaixa.Relatorios.Application.Relatorios.Repositorios;

public interface IRelatoriosRepositorio
{
    Task<VendaConcluidaProjetada?> ObterVendaAsync(Guid vendaId, CancellationToken cancellationToken);

    Task AdicionarVendaAsync(VendaConcluidaProjetada venda, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<VendaConcluidaProjetada>> ListarVendasAsync(CancellationToken cancellationToken);
}
