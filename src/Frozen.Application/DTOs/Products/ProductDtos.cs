namespace Frozen.Application.DTOs.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string Sku,
    decimal Price,
    decimal? CompareAtPrice,
    decimal CostPrice,
    int StockQuantity,
    string? ImageUrl,
    bool IsActive,
    bool IsFeatured,
    Guid CategoryId,
    string CategoryName,
    Guid SupplierId,
    string SupplierName,
    DateTime CreatedAt);

public record CreateProductRequest(
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    decimal? CompareAtPrice,
    decimal CostPrice,
    int StockQuantity,
    string? ImageUrl,
    bool IsFeatured,
    Guid CategoryId,
    Guid SupplierId);

public record UpdateProductRequest(
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    decimal? CompareAtPrice,
    decimal CostPrice,
    int StockQuantity,
    string? ImageUrl,
    bool IsActive,
    bool IsFeatured,
    Guid CategoryId,
    Guid SupplierId);
