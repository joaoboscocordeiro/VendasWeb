using FrenteCaixa.BuildingBlocks.Inbox;
using FrenteCaixa.Relatorios.Application.Relatorios.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Relatorios.Infrastructure.Persistencia.Repositorios;

public sealed class InboxRepositorioRelatorios : IInboxRepositorioRelatorios
{
    private readonly RelatoriosDbContext _dbContext;

    public InboxRepositorioRelatorios(RelatoriosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> JaFoiProcessadaAsync(Guid mensagemId, CancellationToken cancellationToken)
    {
        return _dbContext.InboxMensagens.AnyAsync(
            mensagem => mensagem.MensagemId == mensagemId,
            cancellationToken);
    }

    public async Task RegistrarProcessamentoAsync(
        Guid mensagemId,
        string routingKey,
        DateTimeOffset processadaEm,
        CancellationToken cancellationToken)
    {
        await _dbContext.InboxMensagens.AddAsync(
            RegistroInbox.Criar(mensagemId, routingKey, processadaEm),
            cancellationToken);
    }
}
