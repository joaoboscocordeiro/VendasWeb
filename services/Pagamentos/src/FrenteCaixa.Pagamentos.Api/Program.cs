var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var nomeServico = "Pagamentos";

app.MapGet("/api/saude", () =>
    Results.Ok(new RespostaSaude(nomeServico, "Operacional")))
    .WithName("ObterSaude");

app.MapGet("/api/info", () =>
    Results.Ok(new RespostaServico(nomeServico, "Registro e validacao de pagamentos")))
    .WithName("ObterInformacoesServico");

app.Run();

internal sealed record RespostaSaude(string Servico, string Status);

internal sealed record RespostaServico(string Servico, string Responsabilidade);
