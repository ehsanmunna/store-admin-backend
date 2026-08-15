using System.Text;
using System.Text.Json;
using Frozen.Application.DTOs.Messaging;
using Frozen.Domain.Entities;
using Frozen.Domain.Enums;
using Frozen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Frozen.Infrastructure.Messaging;

public class OrderEventConsumer : BackgroundService
{
    private const string OrdersExchange = "frozen.orders";
    private const string OrdersQueue = "frozen.admin.orders";
    private const string OrderCreatedKey = "order.created";

    private readonly RabbitMqSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderEventConsumer> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private IConnection? _connection;
    private IModel? _channel;

    public OrderEventConsumer(
        IOptions<RabbitMqSettings> options,
        IServiceProvider serviceProvider,
        ILogger<OrderEventConsumer> logger)
    {
        _settings = options.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WaitUntilReady(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ConnectAndConsume(stoppingToken);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order consumer error, restarting in 5s");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task WaitUntilReady(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_settings.Uri),
                    AutomaticRecoveryEnabled = true
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(OrdersExchange, "topic", durable: true);
                _channel.QueueDeclare(OrdersQueue, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(OrdersQueue, OrdersExchange, OrderCreatedKey);
                _logger.LogInformation("Order consumer connected to RabbitMQ");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Waiting for RabbitMQ, retrying in 3s...");
                await Task.Delay(3000, stoppingToken);
            }
        }
    }

    private void ConnectAndConsume(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel!);
        consumer.Received += async (_, ea) =>
        {
            if (ea.RoutingKey != OrderCreatedKey)
            {
                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                return;
            }

            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(json, _jsonOptions);
                if (orderEvent is null)
                {
                    _logger.LogWarning("Failed to deserialize order.created event");
                    _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                await HandleOrderCreatedAsync(orderEvent);
                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                _logger.LogInformation("Processed order.created for {OrderNumber}", orderEvent.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process order.created event");
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel!.BasicConsume(queue: OrdersQueue, autoAck: false, consumer: consumer);
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedEvent orderEvent)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existing = await dbContext.Orders
            .FirstOrDefaultAsync(o => o.OrderNumber == orderEvent.OrderNumber);

        if (existing is not null)
        {
            _logger.LogDebug("Order {OrderNumber} already exists, skipping", orderEvent.OrderNumber);
            return;
        }

        var order = new Order
        {
            OrderNumber = orderEvent.OrderNumber,
            UserId = Guid.Empty,
            Status = OrderStatus.Pending,
            ShippingFullName = orderEvent.ShippingAddress.FullName,
            ShippingAddressLine1 = orderEvent.ShippingAddress.AddressLine1,
            ShippingAddressLine2 = orderEvent.ShippingAddress.AddressLine2,
            ShippingCity = orderEvent.ShippingAddress.City,
            ShippingState = "",
            ShippingPostalCode = orderEvent.ShippingAddress.PostalCode,
            ShippingCountry = "",
            ShippingPhone = orderEvent.ShippingAddress.Phone,
            Notes = $"Customer: {orderEvent.CustomerName} ({orderEvent.CustomerEmail})",
            SubTotal = orderEvent.Items.Sum(i => i.UnitPrice * i.Quantity),
            ShippingCost = 0,
            Tax = 0,
            TotalAmount = orderEvent.TotalAmount
        };

        foreach (var item in orderEvent.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
