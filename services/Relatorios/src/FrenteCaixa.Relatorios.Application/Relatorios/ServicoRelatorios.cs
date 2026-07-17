using FrenteCaixa.Relatorios.Application.Relatorios.Contratos;
using FrenteCaixa.Relatorios.Application.Relatorios.Interfaces;
using FrenteCaixa.Relatorios.Application.Relatorios.Repositorios;
using FrenteCaixa.Relatorios.Domain.Relatorios;

namespace FrenteCaixa.Relatorios.Application.Relatorios;

public sealed class ServicoRelatorios : IServicoRelatorios
{
    private readonly IRelatoriosRepositorio _relatorios;

    public ServicoRelatorios(IRelatoriosRepositorio relatorios)
    {
        _relatorios = relatorios;
    }

    public async Task<IReadOnlyCollection<VendaRelatorioResponse>> ListarVendasAsync(
        CancellationToken cancellationToken)
    {
        var vendas = await _relatorios.ListarVendasAsync(cancellationToken);

        return vendas.Select(MapearVenda).ToArray();
    }

    public async Task<ResumoFinanceiroResponse> ObterResumoFinanceiroAsync(
        CancellationToken cancellationToken)
    {
        var vendas = await _relatorios.ListarVendasAsync(cancellationToken);
        var totais = vendas
            .GroupBy(venda => venda.FormaPagamento)
            .OrderBy(grupo => grupo.Key)
            .Select(grupo => new TotalPorFormaPagamentoResponse(
                grupo.Key,
                grupo.Count(),
                grupo.Sum(venda => venda.ValorTotal)))
            .ToArray();

        return new ResumoFinanceiroResponse(
            vendas.Count,
            vendas.Sum(venda => venda.ValorTotal),
            totais);
    }

    private static VendaRelatorioResponse MapearVenda(VendaConcluidaProjetada venda)
    {
        return new VendaRelatorioResponse(
            venda.VendaId,
            venda.CaixaId,
            venda.OperadorId,
            venda.PagamentoId,
            venda.FormaPagamento,
            venda.ValorTotal,
            venda.ConcluidaEm,
            venda.Itens
                .OrderBy(item => item.DescricaoProduto)
                .Select(item => new ItemVendaRelatorioResponse(
                    item.ProdutoId,
                    item.DescricaoProduto,
                    item.Quantidade,
                    item.PrecoUnitario,
                    item.Subtotal))
                .ToArray());
    }
}
