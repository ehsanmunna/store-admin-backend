namespace Frozen.Application.DTOs.Categories;

public record CategoryDto(Guid Id, string Name, string Slug, string? Description, Guid? ParentCategoryId, int ProductCount);

public record CreateCategoryRequest(string Name, string? Description, Guid? ParentCategoryId);

public record UpdateCategoryRequest(string Name, string? Description, Guid? ParentCategoryId);
