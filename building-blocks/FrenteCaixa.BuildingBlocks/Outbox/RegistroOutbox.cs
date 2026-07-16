using FrenteCaixa.BuildingBlocks.Mensageria;

namespace FrenteCaixa.BuildingBlocks.Outbox;

public sealed class RegistroOutbox
{
    private RegistroOutbox()
    {
        RoutingKey = string.Empty;
        Tipo = string.Empty;
        PayloadJson = string.Empty;
    }

    private RegistroOutbox(Guid id, string routingKey, string tipo, int versao, string payloadJson, DateTimeOffset criadoEm)
    {
        Id = id;
        RoutingKey = routingKey;
        Tipo = tipo;
        Versao = versao;
        PayloadJson = payloadJson;
        CriadoEm = criadoEm;
    }

    public Guid Id { get; private set; }

    public string RoutingKey { get; private set; }

    public string Tipo { get; private set; }

    public int Versao { get; private set; }

    public string PayloadJson { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset? ProcessadoEm { get; private set; }

    public int Tentativas { get; private set; }

    public string? UltimoErro { get; private set; }

    public bool EstaPendente => ProcessadoEm is null;

    public static RegistroOutbox Criar(
        string routingKey,
        string tipo,
        int versao,
        string payloadJson,
        DateTimeOffset criadoEm)
    {
        return new RegistroOutbox(Guid.NewGuid(), routingKey, tipo, versao, payloadJson, criadoEm);
    }

    public EnvelopeEventoIntegracao ParaEnvelope()
    {
        return new EnvelopeEventoIntegracao(Id, RoutingKey, Tipo, Versao, CriadoEm, PayloadJson);
    }

    public void MarcarComoProcessado(DateTimeOffset processadoEm)
    {
        ProcessadoEm = processadoEm;
        UltimoErro = null;
    }

    public void RegistrarFalha(string erro)
    {
        Tentativas++;
        UltimoErro = erro;
    }
}
