using Frozen.Application.DTOs.Suppliers;
using Frozen.Application.Exceptions;
using Frozen.Application.Interfaces;
using Frozen.Domain.Entities;
using Frozen.Domain.Interfaces;

namespace Frozen.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;

    public SupplierService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SupplierDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var suppliers = await _unitOfWork.Suppliers.GetAllAsync(cancellationToken);
        return suppliers.Select(ToDto).ToList();
    }

    public async Task<SupplierDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Supplier), id);
        return ToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = new Supplier
        {
            Name = request.Name,
            ContactEmail = request.ContactEmail,
            Website = request.Website,
            Notes = request.Notes
        };

        await _unitOfWork.Suppliers.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(supplier);
    }

    public async Task<SupplierDto> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Supplier), id);

        supplier.Name = request.Name;
        supplier.ContactEmail = request.ContactEmail;
        supplier.Website = request.Website;
        supplier.Notes = request.Notes;
        supplier.IsActive = request.IsActive;
        supplier.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(supplier);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Supplier), id);

        _unitOfWork.Suppliers.Remove(supplier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static SupplierDto ToDto(Supplier s) =>
        new(s.Id, s.Name, s.ContactEmail, s.Website, s.Notes, s.IsActive, s.CreatedAt);
}
