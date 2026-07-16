var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var nomeServico = "Estoque";

app.MapGet("/api/saude", () =>
    Results.Ok(new RespostaSaude(nomeServico, "Operacional")))
    .WithName("ObterSaude");

app.MapGet("/api/info", () =>
    Results.Ok(new RespostaServico(nomeServico, "Saldos, movimentos e deducao de estoque")))
    .WithName("ObterInformacoesServico");

app.Run();

internal sealed record RespostaSaude(string Servico, string Status);

internal sealed record RespostaServico(string Servico, string Responsabilidade);
