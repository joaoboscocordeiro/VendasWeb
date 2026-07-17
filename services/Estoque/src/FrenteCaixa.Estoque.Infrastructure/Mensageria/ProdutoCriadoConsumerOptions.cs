namespace FrenteCaixa.Estoque.Infrastructure.Mensageria;

public sealed class ProdutoCriadoConsumerOptions
{
    public const string Secao = "RabbitMqConsumers:ProdutoCriado";

    public bool Habilitado { get; set; } = true;

    public string QueueName { get; set; } = "estoque.catalogo.produto-criado";

    public string RoutingKey { get; set; } = "catalogo.produto-criado.v1";
}
