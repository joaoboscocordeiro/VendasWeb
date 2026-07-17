using FrenteCaixa.BuildingBlocks.Outbox;
using FrenteCaixa.CatalogoProdutos.Infrastructure.Persistencia;
using FrenteCaixa.CatalogoProdutos.Infrastructure.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace FrenteCaixa.CatalogoProdutos.Tests;

public sealed class OutboxRepositorioTests
{
    [Fact]
    public async Task catalog_outbox_repository_returns_pending_messages()
    {
        var options = new DbContextOptionsBuilder<CatalogoProdutosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new CatalogoProdutosDbContext(options);
        var primeira = RegistroOutbox.Criar(
            "catalogo.produto-criado.v1",
            "ProdutoCriado",
            1,
            "{\"produtoId\":\"11111111-1111-1111-1111-111111111111\"}",
            DateTimeOffset.UtcNow.AddMinutes(-2));
        var processada = RegistroOutbox.Criar(
            "catalogo.produto-criado.v1",
            "ProdutoCriado",
            1,
            "{\"produtoId\":\"22222222-2222-2222-2222-222222222222\"}",
            DateTimeOffset.UtcNow.AddMinutes(-1));
        var segunda = RegistroOutbox.Criar(
            "catalogo.produto-criado.v1",
            "ProdutoCriado",
            1,
            "{\"produtoId\":\"33333333-3333-3333-3333-333333333333\"}",
            DateTimeOffset.UtcNow);

        processada.MarcarComoProcessado(DateTimeOffset.UtcNow);
        dbContext.OutboxMensagens.AddRange(segunda, processada, primeira);
        await dbContext.SaveChangesAsync();

        var repositorio = new OutboxRepositorio(dbContext);
        var pendentes = await repositorio.ObterPendentesAsync(10, CancellationToken.None);

        Assert.Equal([primeira.Id, segunda.Id], pendentes.Select(mensagem => mensagem.Id));
    }
}
