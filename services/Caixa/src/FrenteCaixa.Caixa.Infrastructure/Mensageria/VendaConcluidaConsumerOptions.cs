namespace FrenteCaixa.Caixa.Infrastructure.Mensageria;

public sealed class VendaConcluidaConsumerOptions
{
    public const string Secao = "RabbitMqConsumers:VendaConcluida";

    public bool Habilitado { get; set; } = true;
    public string QueueName { get; set; } = "caixa.vendas.venda-concluida";
    public string RoutingKey { get; set; } = "vendas.venda-concluida.v1";
}
