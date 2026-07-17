using FrenteCaixa.Relatorios.Application.Relatorios.Interfaces;
using FrenteCaixa.Relatorios.Application.Relatorios.Repositorios;
using FrenteCaixa.Relatorios.Domain.Relatorios;

namespace FrenteCaixa.Relatorios.Application.Relatorios.Eventos;

public sealed class ProcessadorVendaConcluida : IProcessadorVendaConcluida
{
    private readonly IRelatoriosRepositorio _relatorios;
    private readonly IInboxRepositorioRelatorios _inbox;
    private readonly IRelogio _relogio;
    private readonly IUnidadeTrabalho _unidadeTrabalho;

    public ProcessadorVendaConcluida(
        IRelatoriosRepositorio relatorios,
        IInboxRepositorioRelatorios inbox,
        IRelogio relogio,
        IUnidadeTrabalho unidadeTrabalho)
    {
        _relatorios = relatorios;
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

        var vendaExistente = await _relatorios.ObterVendaAsync(evento.VendaId, cancellationToken);

        if (vendaExistente is null)
        {
            var venda = VendaConcluidaProjetada.Criar(
                evento.VendaId,
                evento.CaixaId,
                evento.OperadorId,
                evento.PagamentoId,
                evento.FormaPagamento.Trim(),
                evento.ValorTotal,
                evento.OcorridoEm,
                _relogio.Agora);

            foreach (var item in evento.Itens)
            {
                venda.AdicionarItem(
                    item.ProdutoId,
                    item.DescricaoProduto.Trim(),
                    item.Quantidade,
                    item.PrecoUnitario,
                    item.Subtotal);
            }

            await _relatorios.AdicionarVendaAsync(venda, cancellationToken);
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
