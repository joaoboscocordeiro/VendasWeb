using FrenteCaixa.CatalogoProdutos.Domain.Produtos;

namespace FrenteCaixa.CatalogoProdutos.Application.Produtos.Eventos;

public interface IRegistradorEventosProduto
{
    Task RegistrarProdutoCriadoAsync(Produto produto, CancellationToken cancellationToken);
}
