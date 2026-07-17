using FrenteCaixa.Estoque.Application.Estoques.Interfaces;
using FrenteCaixa.Estoque.Application.Estoques.Repositorios;
using FrenteCaixa.Estoque.Domain.Estoques;

namespace FrenteCaixa.Estoque.Application.Estoques.Eventos;

public sealed class ProcessadorProdutoCriado : IProcessadorProdutoCriado
{
    private readonly IEstoqueRepositorio _estoque;
    private readonly IInboxRepositorioEstoque _inbox;
    private readonly IRelogio _relogio;
    private readonly IUnidadeTrabalho _unidadeTrabalho;

    public ProcessadorProdutoCriado(
        IEstoqueRepositorio estoque,
        IInboxRepositorioEstoque inbox,
        IRelogio relogio,
        IUnidadeTrabalho unidadeTrabalho)
    {
        _estoque = estoque;
        _inbox = inbox;
        _relogio = relogio;
        _unidadeTrabalho = unidadeTrabalho;
    }

    public async Task ProcessarAsync(
        Guid mensagemId,
        string routingKey,
        ProdutoCriadoEvento evento,
        CancellationToken cancellationToken)
    {
        if (mensagemId == Guid.Empty)
        {
            throw new ArgumentException("MensagemId e obrigatorio.", nameof(mensagemId));
        }

        if (evento.ProdutoId == Guid.Empty)
        {
            throw new ArgumentException("ProdutoId e obrigatorio.", nameof(evento));
        }

        if (await _inbox.JaFoiProcessadaAsync(mensagemId, cancellationToken))
        {
            return;
        }

        var saldo = await _estoque.ObterSaldoPorProdutoAsync(evento.ProdutoId, cancellationToken);

        if (saldo is null)
        {
            saldo = SaldoProduto.Criar(evento.ProdutoId, evento.OcorridoEm);
            await _estoque.AdicionarSaldoAsync(saldo, cancellationToken);
        }

        await _inbox.RegistrarProcessamentoAsync(mensagemId, routingKey, _relogio.Agora, cancellationToken);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }
}
