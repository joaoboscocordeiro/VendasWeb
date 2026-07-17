namespace FrenteCaixa.Caixa.Application.Caixas.Repositorios;

public interface IInboxRepositorioCaixa
{
    Task<bool> JaFoiProcessadaAsync(Guid mensagemId, CancellationToken cancellationToken);

    Task RegistrarProcessamentoAsync(
        Guid mensagemId,
        string routingKey,
        DateTimeOffset processadaEm,
        CancellationToken cancellationToken);
}
