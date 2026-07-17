using System.Text.Json;
using FrenteCaixa.BuildingBlocks.Outbox;
using FrenteCaixa.CatalogoProdutos.Application.Produtos.Eventos;
using FrenteCaixa.CatalogoProdutos.Domain.Produtos;
using FrenteCaixa.CatalogoProdutos.Infrastructure.Persistencia;

namespace FrenteCaixa.CatalogoProdutos.Infrastructure.Mensageria;

public sealed class RegistradorEventosProdutoOutbox : IRegistradorEventosProduto
{
    public const string RoutingKeyProdutoCriado = "catalogo.produto-criado.v1";
    public const string TipoProdutoCriado = "ProdutoCriado";
    public const int VersaoProdutoCriado = 1;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly CatalogoProdutosDbContext _dbContext;

    public RegistradorEventosProdutoOutbox(CatalogoProdutosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RegistrarProdutoCriadoAsync(Produto produto, CancellationToken cancellationToken)
    {
        var evento = new ProdutoCriadoEvento(
            produto.Id,
            produto.Descricao,
            produto.CodigoBarrasEan,
            produto.CriadoEm);

        var registro = RegistroOutbox.Criar(
            RoutingKeyProdutoCriado,
            TipoProdutoCriado,
            VersaoProdutoCriado,
            JsonSerializer.Serialize(evento, JsonOptions),
            produto.CriadoEm);

        await _dbContext.OutboxMensagens.AddAsync(registro, cancellationToken);
    }
}
