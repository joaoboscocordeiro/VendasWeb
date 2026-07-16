namespace FrenteCaixa.BuildingBlocks.Outbox;

public sealed class OutboxOptions
{
    public const string Secao = "Outbox";

    public bool Habilitado { get; set; } = true;

    public int IntervaloSegundos { get; set; } = 10;

    public int QuantidadePorLote { get; set; } = 25;
}
