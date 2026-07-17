using System.Text.Json;
using FrenteCaixa.BuildingBlocks.Outbox;
using FrenteCaixa.Vendas.Application.Vendas.Eventos;
using FrenteCaixa.Vendas.Domain.Vendas;
using FrenteCaixa.Vendas.Infrastructure.Persistencia;

namespace FrenteCaixa.Vendas.Infrastructure.Mensageria;

public sealed class RegistradorEventosVendaOutbox : IRegistradorEventosVenda
{
    public const string RoutingKeyVendaConcluida = "vendas.venda-concluida.v1";
    public const string TipoVendaConcluida = "VendaConcluida";
    public const int VersaoVendaConcluida = 1;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly VendasDbContext _dbContext;

    public RegistradorEventosVendaOutbox(VendasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RegistrarVendaConcluidaAsync(
        Venda venda,
        Guid pagamentoId,
        string formaPagamento,
        CancellationToken cancellationToken)
    {
        var itens = venda.Itens
            .OrderBy(item => item.CriadoEm)
            .Select(item => new ItemVendaConcluidaEvento(
                item.ProdutoId,
                item.DescricaoProduto,
                item.Quantidade,
                item.PrecoUnitario,
                item.Subtotal))
            .ToArray();
        var evento = new VendaConcluidaEvento(
            venda.Id,
            venda.CaixaId,
            venda.OperadorId,
            venda.Total,
            pagamentoId,
            formaPagamento,
            itens,
            venda.AtualizadaEm);
        var registro = RegistroOutbox.Criar(
            RoutingKeyVendaConcluida,
            TipoVendaConcluida,
            VersaoVendaConcluida,
            JsonSerializer.Serialize(evento, JsonOptions),
            venda.AtualizadaEm);

        await _dbContext.OutboxMensagens.AddAsync(registro, cancellationToken);
    }
}
