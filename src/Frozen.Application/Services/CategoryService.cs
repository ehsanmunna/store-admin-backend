using Frozen.Application.DTOs.Categories;
using Frozen.Application.DTOs.Messaging;
using Frozen.Application.Exceptions;
using Frozen.Application.Interfaces;
using Frozen.Domain.Entities;
using Frozen.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Frozen.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICatalogEventPublisher _eventPublisher;

    public CategoryService(IUnitOfWork unitOfWork, ICatalogEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Categories.Query()
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.Description, c.ParentCategoryId, c.Products.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);
        return ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Name = request.Name,
            Slug = Slugify(request.Name),
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishCategoryCreatedAsync(ToSyncEvent(category), cancellationToken);
        return ToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        category.Name = request.Name;
        category.Slug = Slugify(request.Name);
        category.Description = request.Description;
        category.ParentCategoryId = request.ParentCategoryId;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishCategoryUpdatedAsync(ToSyncEvent(category), cancellationToken);
        return ToDto(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static CategoryDto ToDto(Category c) =>
        new(c.Id, c.Name, c.Slug, c.Description, c.ParentCategoryId, c.Products.Count);

    private static CategorySyncEvent ToSyncEvent(Category c) =>
        new(c.Id, c.Name, c.Slug, c.Description, c.ParentCategoryId);

    private static string Slugify(string name) =>
        name.Trim().ToLowerInvariant().Replace(" ", "-");
}
