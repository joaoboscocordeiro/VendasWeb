namespace FrenteCaixa.Relatorios.Application.Relatorios.Repositorios;

public interface IInboxRepositorioRelatorios
{
    Task<bool> JaFoiProcessadaAsync(Guid mensagemId, CancellationToken cancellationToken);

    Task RegistrarProcessamentoAsync(
        Guid mensagemId,
        string routingKey,
        DateTimeOffset processadaEm,
        CancellationToken cancellationToken);
}
