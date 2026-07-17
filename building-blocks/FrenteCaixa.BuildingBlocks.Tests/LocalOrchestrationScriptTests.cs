namespace FrenteCaixa.BuildingBlocks.Tests;

public sealed class LocalOrchestrationScriptTests
{
    [Fact]
    public void local_orchestration_script_declares_all_services_and_ports()
    {
        var script = ReadScript();
        var expected = new Dictionary<string, int>
        {
            ["Identidade"] = 5227,
            ["CatalogoProdutos"] = 5089,
            ["Estoque"] = 5252,
            ["Caixa"] = 5062,
            ["Vendas"] = 5165,
            ["Pagamentos"] = 5216,
            ["Relatorios"] = 5002,
            ["BFF"] = 5265
        };

        foreach (var service in expected)
        {
            Assert.Contains($"Name = '{service.Key}'", script);
            Assert.Contains($"Port = {service.Value}", script);
        }
    }

    [Fact]
    public void local_orchestration_script_supports_health_only_mode()
    {
        var script = ReadScript();

        Assert.Contains("[switch]$HealthOnly", script);
        Assert.Contains("/api/saude", script);
        Assert.Contains("Test-HealthChecks", script);
    }

    [Fact]
    public void local_orchestration_script_stops_only_recorded_processes()
    {
        var script = ReadScript();

        Assert.Contains("[switch]$Stop", script);
        Assert.Contains(".codex-run\\frente-caixa-local", script);
        Assert.Contains("processes.json", script);
        Assert.Contains("Stop-RecordedProcesses", script);
        Assert.Contains("Get-Content $ProcessFile", script);
    }

    private static string ReadScript()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FrenteCaixa.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        var scriptPath = Path.Combine(directory.FullName, "scripts", "local", "run-microsservicos.ps1");

        Assert.True(File.Exists(scriptPath), $"Script nao encontrado em {scriptPath}");

        return File.ReadAllText(scriptPath);
    }
}
