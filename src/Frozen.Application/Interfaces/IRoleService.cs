using Frozen.Application.DTOs.Roles;

namespace Frozen.Application.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleDto> RenameAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> RoleExistsAsync(string name, CancellationToken cancellationToken = default);
}