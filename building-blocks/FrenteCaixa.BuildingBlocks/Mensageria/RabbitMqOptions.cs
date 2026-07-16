namespace FrenteCaixa.BuildingBlocks.Mensageria;

public sealed class RabbitMqOptions
{
    public const string Secao = "RabbitMq";

    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string VirtualHost { get; set; } = "/";

    public string ExchangeName { get; set; } = "frente-caixa.eventos";
}
