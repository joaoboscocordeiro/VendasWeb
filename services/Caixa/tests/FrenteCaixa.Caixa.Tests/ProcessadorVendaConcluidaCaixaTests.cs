using FrenteCaixa.Caixa.Application.Caixas.Eventos;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.Caixa.Tests;

public sealed class ProcessadorVendaConcluidaCaixaTests
{
    [Fact]
    public async Task cash_processor_projects_completed_sale()
    {
        using var factory = new CaixaApiFactory();
        var caixa = await factory.SemearCaixaAbertoAsync();
        var mensagemId = Guid.NewGuid();
        var evento = CriarEvento(caixa.Id);

        using var scope = factory.Services.CreateScope();
        var processador = scope.ServiceProvider.GetRequiredService<IProcessadorVendaConcluidaCaixa>();
        var banco = scope.ServiceProvider.GetRequiredService<BancoCaixaEmMemoria>();

        await processador.ProcessarAsync(
            mensagemId,
            "vendas.venda-concluida.v1",
            evento,
            CancellationToken.None);

        Assert.Single(banco.VendasProjetadas);
        Assert.Equal(evento.VendaId, banco.VendasProjetadas.Single().VendaId);
        Assert.Contains(mensagemId, banco.MensagensProcessadas);
    }

    [Fact]
    public async Task cash_processor_ignores_duplicate_completed_sale_message()
    {
        using var factory = new CaixaApiFactory();
        var caixa = await factory.SemearCaixaAbertoAsync();
        var mensagemId = Guid.NewGuid();
        var evento = CriarEvento(caixa.Id);

        using var scope = factory.Services.CreateScope();
        var processador = scope.ServiceProvider.GetRequiredService<IProcessadorVendaConcluidaCaixa>();
        var banco = scope.ServiceProvider.GetRequiredService<BancoCaixaEmMemoria>();

        await processador.ProcessarAsync(
            mensagemId,
            "vendas.venda-concluida.v1",
            evento,
            CancellationToken.None);
        await processador.ProcessarAsync(
            mensagemId,
            "vendas.venda-concluida.v1",
            evento,
            CancellationToken.None);

        Assert.Single(banco.VendasProjetadas);
        Assert.Single(banco.MensagensProcessadas);
    }

    private static VendaConcluidaEvento CriarEvento(Guid caixaId)
    {
        return new VendaConcluidaEvento(
            Guid.NewGuid(),
            caixaId,
            CaixaApiFactory.OperadorPadraoId,
            20m,
            Guid.NewGuid(),
            "Dinheiro",
            [],
            DateTimeOffset.UtcNow);
    }
}
