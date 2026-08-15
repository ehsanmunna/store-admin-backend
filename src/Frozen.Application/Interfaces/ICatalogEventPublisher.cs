using Frozen.Application.DTOs.Messaging;

namespace Frozen.Application.Interfaces;

public interface ICatalogEventPublisher
{
    Task PublishProductCreatedAsync(ProductSyncEvent payload, CancellationToken cancellationToken = default);
    Task PublishProductUpdatedAsync(ProductSyncEvent payload, CancellationToken cancellationToken = default);
    Task PublishProductDeletedAsync(Guid id, CancellationToken cancellationToken = default);
    Task PublishCategoryCreatedAsync(CategorySyncEvent payload, CancellationToken cancellationToken = default);
    Task PublishCategoryUpdatedAsync(CategorySyncEvent payload, CancellationToken cancellationToken = default);
}