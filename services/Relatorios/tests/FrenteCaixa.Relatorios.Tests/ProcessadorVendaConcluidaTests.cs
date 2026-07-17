using FrenteCaixa.Relatorios.Application.Relatorios.Eventos;
using Microsoft.Extensions.DependencyInjection;

namespace FrenteCaixa.Relatorios.Tests;

public sealed class ProcessadorVendaConcluidaTests
{
    [Fact]
    public async Task reports_processor_projects_completed_sale()
    {
        using var factory = new RelatoriosApiFactory();
        using var scope = factory.Services.CreateScope();
        var processador = scope.ServiceProvider.GetRequiredService<IProcessadorVendaConcluida>();
        var banco = scope.ServiceProvider.GetRequiredService<BancoRelatoriosEmMemoria>();
        var mensagemId = Guid.NewGuid();

        await processador.ProcessarAsync(
            mensagemId,
            "vendas.venda-concluida.v1",
            CriarEvento(),
            CancellationToken.None);

        var venda = Assert.Single(banco.Vendas);
        Assert.Equal(35.90m, venda.ValorTotal);
        Assert.Contains(mensagemId, banco.MensagensProcessadas);
    }

    [Fact]
    public async Task reports_processor_ignores_duplicate_completed_sale_message()
    {
        using var factory = new RelatoriosApiFactory();
        using var scope = factory.Services.CreateScope();
        var processador = scope.ServiceProvider.GetRequiredService<IProcessadorVendaConcluida>();
        var banco = scope.ServiceProvider.GetRequiredService<BancoRelatoriosEmMemoria>();
        var mensagemId = Guid.NewGuid();
        var evento = CriarEvento();

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

        Assert.Single(banco.Vendas);
        Assert.Single(banco.MensagensProcessadas);
    }

    private static VendaConcluidaEvento CriarEvento()
    {
        return new VendaConcluidaEvento(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            35.90m,
            Guid.NewGuid(),
            "Dinheiro",
            [
                new ItemVendaConcluidaEvento(
                    Guid.NewGuid(),
                    "Cafe Torrado 500g",
                    2m,
                    10m,
                    20m),
                new ItemVendaConcluidaEvento(
                    Guid.NewGuid(),
                    "Acucar Cristal 1kg",
                    1m,
                    15.90m,
                    15.90m)
            ],
            DateTimeOffset.UtcNow);
    }
}
