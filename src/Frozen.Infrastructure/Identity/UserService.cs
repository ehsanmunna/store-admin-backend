using Frozen.Application.DTOs.Users;
using Frozen.Application.Exceptions;
using Frozen.Application.Interfaces;
using Frozen.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Frozen.Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        var result = new List<UserDto>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserDto(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, roles.ToList()));
        }

        return result;
    }

    public async Task<UserDto> CreateAdminAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new ValidationException("A user with this email already exists.");

        var roles = request.Roles?
            .Select(r => r.Trim())
            .Where(r => r.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (roles is null || roles.Count == 0)
        {
            roles = new List<string> { UserRoles.Admin };
        }
        else
        {
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    throw new ValidationException($"Role '{role}' does not exist.");
            }
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ValidationException(string.Join(" ", result.Errors.Select(e => e.Description)));

        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
            await _roleManager.CreateAsync(new IdentityRole<Guid>(UserRoles.Admin));

        foreach (var role in roles)
            await _userManager.AddToRoleAsync(user, role);

        return new UserDto(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, roles);
    }

    public async Task SetRolesAsync(Guid userId, IReadOnlyList<string> roles, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException(nameof(ApplicationUser), userId);

        var normalized = roles
            .Select(r => r.Trim())
            .Where(r => r.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
            throw new ValidationException("A user must be assigned at least one role.");

        foreach (var role in normalized)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                throw new ValidationException($"Role '{role}' does not exist.");
        }

        var current = await _userManager.GetRolesAsync(user);
        if (current.Count > 0)
        {
            var remove = await _userManager.RemoveFromRolesAsync(user, current);
            if (!remove.Succeeded)
                throw new ValidationException(string.Join(" ", remove.Errors.Select(e => e.Description)));
        }

        var add = await _userManager.AddToRolesAsync(user, normalized);
        if (!add.Succeeded)
            throw new ValidationException(string.Join(" ", add.Errors.Select(e => e.Description)));
    }

    public async Task ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException(nameof(ApplicationUser), userId);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded)
            throw new ValidationException(string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
