namespace Frozen.Application.DTOs.Suppliers;

public record SupplierDto(Guid Id, string Name, string? ContactEmail, string? Website, string? Notes, bool IsActive, DateTime CreatedAt);

public record CreateSupplierRequest(string Name, string? ContactEmail, string? Website, string? Notes);

public record UpdateSupplierRequest(string Name, string? ContactEmail, string? Website, string? Notes, bool IsActive);
