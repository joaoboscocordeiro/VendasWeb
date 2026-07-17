using FrenteCaixa.BuildingBlocks.Inbox;
using FrenteCaixa.Estoque.Application.Estoques.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.Estoque.Infrastructure.Persistencia.Repositorios;

public sealed class InboxRepositorioEstoque : IInboxRepositorioEstoque
{
    private readonly EstoqueDbContext _dbContext;

    public InboxRepositorioEstoque(EstoqueDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> JaFoiProcessadaAsync(Guid mensagemId, CancellationToken cancellationToken)
    {
        return _dbContext.InboxMensagens.AnyAsync(
            registro => registro.MensagemId == mensagemId,
            cancellationToken);
    }

    public async Task RegistrarProcessamentoAsync(
        Guid mensagemId,
        string routingKey,
        DateTimeOffset consumidoEm,
        CancellationToken cancellationToken)
    {
        var registro = RegistroInbox.Criar(mensagemId, routingKey, consumidoEm);

        await _dbContext.InboxMensagens.AddAsync(registro, cancellationToken);
    }
}
