using Frozen.Application.Common;
using Frozen.Application.DTOs.Products;
using Frozen.Application.Exceptions;
using Frozen.Application.Interfaces;
using Frozen.Domain.Entities;
using Frozen.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Frozen.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(PagedRequest request, Guid? categoryId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Products.Query();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Name.Contains(request.Search) || p.Sku.Contains(request.Search));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => ToDto(p))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.Query()
            .Where(p => p.Id == id)
            .Select(p => ToDto(p))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        return product;
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);
        _ = await _unitOfWork.Suppliers.GetByIdAsync(request.SupplierId, cancellationToken)
            ?? throw new NotFoundException(nameof(Supplier), request.SupplierId);

        var product = new Product
        {
            Name = request.Name,
            Slug = Slugify(request.Name),
            Description = request.Description,
            Sku = request.Sku,
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            CostPrice = request.CostPrice,
            StockQuantity = request.StockQuantity,
            ImageUrl = request.ImageUrl,
            IsFeatured = request.IsFeatured,
            CategoryId = request.CategoryId,
            SupplierId = request.SupplierId
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(product.Id, cancellationToken);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        _ = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);
        _ = await _unitOfWork.Suppliers.GetByIdAsync(request.SupplierId, cancellationToken)
            ?? throw new NotFoundException(nameof(Supplier), request.SupplierId);

        product.Name = request.Name;
        product.Slug = Slugify(request.Name);
        product.Description = request.Description;
        product.Sku = request.Sku;
        product.Price = request.Price;
        product.CompareAtPrice = request.CompareAtPrice;
        product.CostPrice = request.CostPrice;
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = request.ImageUrl;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.CategoryId = request.CategoryId;
        product.SupplierId = request.SupplierId;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(product.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ProductDto ToDto(Product p) => new(
        p.Id, p.Name, p.Slug, p.Description, p.Sku, p.Price, p.CompareAtPrice, p.CostPrice,
        p.StockQuantity, p.ImageUrl, p.IsActive, p.IsFeatured,
        p.CategoryId, p.Category.Name, p.SupplierId, p.Supplier.Name, p.CreatedAt);

    private static string Slugify(string name) =>
        name.Trim().ToLowerInvariant().Replace(" ", "-");
}
