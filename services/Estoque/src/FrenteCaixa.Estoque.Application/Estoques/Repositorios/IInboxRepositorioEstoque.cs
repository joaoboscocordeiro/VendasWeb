namespace FrenteCaixa.Estoque.Application.Estoques.Repositorios;

public interface IInboxRepositorioEstoque
{
    Task<bool> JaFoiProcessadaAsync(Guid mensagemId, CancellationToken cancellationToken);

    Task RegistrarProcessamentoAsync(
        Guid mensagemId,
        string routingKey,
        DateTimeOffset consumidoEm,
        CancellationToken cancellationToken);
}
