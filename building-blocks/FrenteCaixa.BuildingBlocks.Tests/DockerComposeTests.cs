namespace FrenteCaixa.BuildingBlocks.Tests;

public sealed class DockerComposeTests
{
    [Fact]
    public void docker_compose_declares_rabbitmq_and_context_databases()
    {
        var raiz = LocalizarRaizRepositorio();
        var compose = File.ReadAllText(Path.Combine(raiz, "docker-compose.yml"));
        var scriptBancos = File.ReadAllText(Path.Combine(
            raiz,
            "docker",
            "postgres",
            "init",
            "01-create-context-databases.sql"));

        Assert.Contains("rabbitmq:", compose);
        Assert.Contains("frente-caixa.eventos", LerAppSettings(Path.Combine(
            raiz,
            "services",
            "Identidade",
            "src",
            "FrenteCaixa.Identidade.Api",
            "appsettings.json")));

        string[] bancos =
        [
            "frente_caixa_identidade",
            "frente_caixa_catalogo",
            "frente_caixa_estoque",
            "frente_caixa_caixa",
            "frente_caixa_vendas",
            "frente_caixa_pagamentos",
            "frente_caixa_relatorios"
        ];

        foreach (var banco in bancos)
        {
            Assert.Contains(banco, scriptBancos);
        }
    }

    private static string LerAppSettings(string caminho)
    {
        return File.ReadAllText(caminho);
    }

    private static string LocalizarRaizRepositorio()
    {
        var diretorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (diretorio is not null && !File.Exists(Path.Combine(diretorio.FullName, "FrenteCaixa.sln")))
        {
            diretorio = diretorio.Parent;
        }

        return diretorio?.FullName
            ?? throw new DirectoryNotFoundException("Raiz do repositorio nao encontrada.");
    }
}
