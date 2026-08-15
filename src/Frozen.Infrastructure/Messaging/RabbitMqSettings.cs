namespace Frozen.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public string Uri { get; set; } = "amqp://guest:guest@localhost:5672";
    public string Exchange { get; set; } = "frozen.catalog";
    public string OrdersExchange { get; set; } = "frozen.orders";
    public string ExchangeType { get; set; } = "topic";
    public bool Durable { get; set; } = true;
}