using FrenteCaixa.Bff.Api.Pdv;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FrenteCaixa.Bff.Tests;

public sealed class BffApiFactory : WebApplicationFactory<Program>
{
    public const string JwtIssuer = "FrenteCaixa.Identidade.Testes";
    public const string JwtAudience = "FrenteCaixa.Backend.Testes";
    public const string JwtKey = "chave-de-testes-com-mais-de-32-bytes-para-bff";

    public FakeBackendHealthClient BackendHealthClient { get; } = new();
    public FakePdvProdutosService ProdutosService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:Chave"] = JwtKey,
                ["PdvBootstrap:Moeda"] = "BRL",
                ["PdvBootstrap:CasasDecimais"] = "2",
                ["PdvBootstrap:PermiteVendaSemEstoque"] = "false",
                ["PdvBootstrap:Servicos:0:Nome"] = "CatalogoProdutos",
                ["PdvBootstrap:Servicos:0:BaseUrl"] = "http://catalogo.local",
                ["PdvBootstrap:Servicos:0:HealthPath"] = "/api/saude",
                ["PdvBootstrap:Servicos:1:Nome"] = "Estoque",
                ["PdvBootstrap:Servicos:1:BaseUrl"] = "http://estoque.local",
                ["PdvBootstrap:Servicos:1:HealthPath"] = "/api/saude"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IBackendHealthClient>();
            services.RemoveAll<IPdvProdutosService>();

            services.AddSingleton(BackendHealthClient);
            services.AddSingleton<IBackendHealthClient>(sp =>
                sp.GetRequiredService<FakeBackendHealthClient>());
            services.AddSingleton(ProdutosService);
            services.AddSingleton<IPdvProdutosService>(sp =>
                sp.GetRequiredService<FakePdvProdutosService>());
        });
    }
}
