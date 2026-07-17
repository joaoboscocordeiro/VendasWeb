using FrenteCaixa.CatalogoProdutos.Application.Produtos.Interfaces;

namespace FrenteCaixa.CatalogoProdutos.Infrastructure.Tempo;

public sealed class RelogioSistema : IRelogio
{
    public DateTimeOffset Agora => DateTimeOffset.UtcNow;
}
