using Frozen.Application.DTOs.Roles;
using Frozen.Application.Exceptions;
using Frozen.Application.Interfaces;
using Frozen.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Frozen.Infrastructure.Identity;

public class RoleService : IRoleService
{
    private static readonly string[] BuiltInRoles = { UserRoles.Admin, UserRoles.SuperAdmin };

    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleService(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles.ToListAsync(cancellationToken);
        var result = new List<RoleDto>(roles.Count);

        foreach (var role in roles)
        {
            var count = (await _userManager.GetUsersInRoleAsync(role.Name!)).Count;
            result.Add(new RoleDto(role.Id, role.Name!, count));
        }

        return result;
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var name = NormalizeName(request.Name);
        if (await _roleManager.RoleExistsAsync(name))
            throw new ValidationException($"A role named '{name}' already exists.");

        var role = new IdentityRole<Guid>(name);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new ValidationException(string.Join(" ", result.Errors.Select(e => e.Description)));

        return new RoleDto(role.Id, role.Name!, 0);
    }

    public async Task<RoleDto> RenameAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new NotFoundException(nameof(IdentityRole<Guid>), roleId);

        if (BuiltInRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase))
            throw new ValidationException($"The built-in role '{role.Name}' cannot be renamed.");

        var name = NormalizeName(request.Name);
        if (string.Equals(role.Name, name, StringComparison.OrdinalIgnoreCase))
            return new RoleDto(role.Id, role.Name!, await CountUsersAsync(role.Name!));

        if (await _roleManager.RoleExistsAsync(name))
            throw new ValidationException($"A role named '{name}' already exists.");

        role.Name = name;
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new ValidationException(string.Join(" ", result.Errors.Select(e => e.Description)));

        return new RoleDto(role.Id, role.Name!, await CountUsersAsync(role.Name!));
    }

    public async Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new NotFoundException(nameof(IdentityRole<Guid>), roleId);

        if (BuiltInRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase))
            throw new ValidationException($"The built-in role '{role.Name}' cannot be deleted.");

        if (await CountUsersAsync(role.Name!) > 0)
            throw new ConflictException($"Role '{role.Name}' is assigned to users and cannot be deleted.");

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            throw new ValidationException(string.Join(" ", result.Errors.Select(e => e.Description)));
    }

    public async Task<bool> RoleExistsAsync(string name, CancellationToken cancellationToken = default)
        => await _roleManager.RoleExistsAsync(NormalizeName(name));

    private static string NormalizeName(string name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ValidationException("Role name cannot be empty.");
        return trimmed;
    }

    private async Task<int> CountUsersAsync(string roleName)
        => (await _userManager.GetUsersInRoleAsync(roleName)).Count;
}