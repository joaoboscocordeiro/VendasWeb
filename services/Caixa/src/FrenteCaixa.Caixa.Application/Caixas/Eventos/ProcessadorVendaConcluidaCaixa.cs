using FrenteCaixa.Caixa.Application.Caixas.Interfaces;
using FrenteCaixa.Caixa.Application.Caixas.Repositorios;
using FrenteCaixa.Caixa.Domain.Caixas;

namespace FrenteCaixa.Caixa.Application.Caixas.Eventos;

public sealed class ProcessadorVendaConcluidaCaixa : IProcessadorVendaConcluidaCaixa
{
    private readonly ICaixaRepositorio _caixas;
    private readonly IInboxRepositorioCaixa _inbox;
    private readonly IRelogio _relogio;
    private readonly IUnidadeTrabalho _unidadeTrabalho;

    public ProcessadorVendaConcluidaCaixa(
        ICaixaRepositorio caixas,
        IInboxRepositorioCaixa inbox,
        IRelogio relogio,
        IUnidadeTrabalho unidadeTrabalho)
    {
        _caixas = caixas;
        _inbox = inbox;
        _relogio = relogio;
        _unidadeTrabalho = unidadeTrabalho;
    }

    public async Task ProcessarAsync(
        Guid mensagemId,
        string routingKey,
        VendaConcluidaEvento evento,
        CancellationToken cancellationToken)
    {
        Validar(mensagemId, evento);

        if (await _inbox.JaFoiProcessadaAsync(mensagemId, cancellationToken))
        {
            return;
        }

        var vendaExistente = await _caixas.ObterVendaProjetadaAsync(evento.VendaId, cancellationToken);

        if (vendaExistente is null)
        {
            var venda = VendaCaixaProjetada.Criar(
                evento.VendaId,
                evento.CaixaId,
                evento.OperadorId,
                evento.PagamentoId,
                evento.FormaPagamento,
                evento.ValorTotal,
                evento.OcorridoEm,
                _relogio.Agora);

            await _caixas.AdicionarVendaProjetadaAsync(venda, cancellationToken);
        }

        await _inbox.RegistrarProcessamentoAsync(mensagemId, routingKey, _relogio.Agora, cancellationToken);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }

    private static void Validar(Guid mensagemId, VendaConcluidaEvento evento)
    {
        if (mensagemId == Guid.Empty)
        {
            throw new ArgumentException("MensagemId e obrigatorio.", nameof(mensagemId));
        }

        if (evento.VendaId == Guid.Empty)
        {
            throw new ArgumentException("VendaId e obrigatorio.", nameof(evento));
        }

        if (evento.CaixaId == Guid.Empty)
        {
            throw new ArgumentException("CaixaId e obrigatorio.", nameof(evento));
        }

        if (evento.OperadorId == Guid.Empty)
        {
            throw new ArgumentException("OperadorId e obrigatorio.", nameof(evento));
        }

        if (evento.ValorTotal <= 0)
        {
            throw new ArgumentException("ValorTotal deve ser maior que zero.", nameof(evento));
        }

        if (string.IsNullOrWhiteSpace(evento.FormaPagamento))
        {
            throw new ArgumentException("FormaPagamento e obrigatoria.", nameof(evento));
        }
    }
}
