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

    [Fact]
    public void local_orchestration_script_seeds_demo_products_and_stock()
    {
        var script = ReadScript();

        Assert.Contains("Seed-DemoProducts", script);
        Assert.Contains("Cafe Torrado Demo 500g", script);
        Assert.Contains("7891000000015", script);
        Assert.Contains("frente_caixa_catalogo", script);
        Assert.Contains("frente_caixa_estoque", script);
        Assert.Contains("ON CONFLICT (\"ProdutoId\") DO UPDATE", script);
    }

    [Fact]
    public void local_orchestration_script_validates_complete_demo_sale_flow()
    {
        var script = ReadScript();

        Assert.Contains("[switch]$SkipDemoFlow", script);
        Assert.Contains("Invoke-DemoSaleSmoke", script);
        Assert.Contains("/pdv/products/by-barcode/", script);
        Assert.Contains("/sales/$($sale.id)/checkout", script);
        Assert.Contains("/pdv/cash-register/current/summary", script);
        Assert.Contains("/admin/reports/sales", script);
    }

    [Fact]
    public void pdv_e2e_script_prepares_local_stack_and_runs_playwright()
    {
        var script = ReadScript("run-pdv-e2e.ps1");

        Assert.Contains("run-microsservicos.ps1", script);
        Assert.Contains("-SkipDemoFlow", script);
        Assert.Contains("npm.cmd", script);
        Assert.Contains("[switch]$SkipBrowserInstall", script);
        Assert.Contains("playwright install chromium", script);
        Assert.Contains("PLAYWRIGHT_BASE_URL", script);
        Assert.Contains("@('playwright', 'test')", script);
    }

    private static string ReadScript()
    {
        return ReadScript("run-microsservicos.ps1");
    }

    private static string ReadScript(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FrenteCaixa.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        var scriptPath = Path.Combine(directory.FullName, "scripts", "local", fileName);

        Assert.True(File.Exists(scriptPath), $"Script nao encontrado em {scriptPath}");

        return File.ReadAllText(scriptPath);
    }
}
