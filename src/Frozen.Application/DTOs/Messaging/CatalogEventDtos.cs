namespace Frozen.Application.DTOs.Messaging;

public record ProductSyncEvent(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string Sku,
    decimal Price,
    decimal? CompareAtPrice,
    int StockQuantity,
    string? ImageUrl,
    bool IsActive,
    bool IsFeatured,
    CategorySyncEvent Category);

public record ProductDeletedEvent(Guid Id);

public record CategorySyncEvent(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId);