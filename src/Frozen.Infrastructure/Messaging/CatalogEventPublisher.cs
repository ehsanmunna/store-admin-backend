using System.Text;
using System.Text.Json;
using Frozen.Application.DTOs.Messaging;
using Frozen.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Frozen.Infrastructure.Messaging;

public class CatalogEventPublisher : ICatalogEventPublisher, IDisposable
{
    private const string ProductCreatedKey = "product.created";
    private const string ProductUpdatedKey = "product.updated";
    private const string ProductDeletedKey = "product.deleted";
    private const string CategoryCreatedKey = "category.created";
    private const string CategoryUpdatedKey = "category.updated";

    private readonly RabbitMqSettings _settings;
    private readonly ILogger<CatalogEventPublisher> _logger;
    private readonly object _gate = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private IConnection? _connection;
    private IModel? _channel;

    public CatalogEventPublisher(IOptions<RabbitMqSettings> options, ILogger<CatalogEventPublisher> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public Task PublishProductCreatedAsync(ProductSyncEvent payload, CancellationToken cancellationToken = default)
        => PublishAsync(ProductCreatedKey, payload, cancellationToken);

    public Task PublishProductUpdatedAsync(ProductSyncEvent payload, CancellationToken cancellationToken = default)
        => PublishAsync(ProductUpdatedKey, payload, cancellationToken);

    public Task PublishProductDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        => PublishAsync(ProductDeletedKey, new ProductDeletedEvent(id), cancellationToken);

    public Task PublishCategoryCreatedAsync(CategorySyncEvent payload, CancellationToken cancellationToken = default)
        => PublishAsync(CategoryCreatedKey, payload, cancellationToken);

    public Task PublishCategoryUpdatedAsync(CategorySyncEvent payload, CancellationToken cancellationToken = default)
        => PublishAsync(CategoryUpdatedKey, payload, cancellationToken);

    private Task PublishAsync<T>(string routingKey, T payload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            var channel = GetChannel();
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, JsonOptions));
            channel.BasicPublish(
                exchange: _settings.Exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: null,
                body: body);
        }, cancellationToken);
    }

    private IModel GetChannel()
    {
        lock (_gate)
        {
            if (_channel is not null && _channel.IsOpen)
                return _channel;

            _channel?.Dispose();
            _connection?.Dispose();

            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_settings.Uri),
                    AutomaticRecoveryEnabled = true
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(
                    exchange: _settings.Exchange,
                    type: _settings.ExchangeType,
                    durable: _settings.Durable,
                    autoDelete: false);

                _logger.LogInformation("Connected to RabbitMQ exchange '{Exchange}'", _settings.Exchange);
                return _channel;
            }
            catch (Exception ex)
            {
                _channel?.Dispose();
                _channel = null;
                _connection?.Dispose();
                _connection = null;

                _logger.LogError(ex, "Failed to connect to RabbitMQ at '{Uri}'", _settings.Uri);
                throw;
            }
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _channel?.Dispose();
            _channel = null;
            _connection?.Dispose();
            _connection = null;
        }
    }
}