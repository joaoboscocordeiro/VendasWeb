using FrenteCaixa.BuildingBlocks.Inbox;
using FrenteCaixa.Caixa.Application.Caixas.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Caixa.Infrastructure.Persistencia.Repositorios;

public sealed class InboxRepositorioCaixa : IInboxRepositorioCaixa
{
    private readonly CaixaDbContext _dbContext;

    public InboxRepositorioCaixa(CaixaDbContext dbContext)
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
