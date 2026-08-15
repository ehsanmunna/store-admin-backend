using Frozen.Domain.Enums;

namespace Frozen.Application.Interfaces;

public interface IOrderEventPublisher
{
    Task PublishOrderStatusUpdatedAsync(string orderNumber, OrderStatus status, CancellationToken cancellationToken = default);
}
