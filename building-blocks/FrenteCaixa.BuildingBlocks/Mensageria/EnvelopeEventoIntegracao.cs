namespace FrenteCaixa.BuildingBlocks.Mensageria;

public sealed record EnvelopeEventoIntegracao(
    Guid Id,
    string RoutingKey,
    string Tipo,
    int Versao,
    DateTimeOffset OcorridoEm,
    string PayloadJson,
    string? CorrelationId = null)
{
    public static EnvelopeEventoIntegracao Criar(
        string routingKey,
        string tipo,
        int versao,
        string payloadJson,
        DateTimeOffset ocorridoEm,
        string? correlationId = null)
    {
        return new EnvelopeEventoIntegracao(
            Guid.NewGuid(),
            routingKey,
            tipo,
            versao,
            ocorridoEm,
            payloadJson,
            correlationId);
    }
}
