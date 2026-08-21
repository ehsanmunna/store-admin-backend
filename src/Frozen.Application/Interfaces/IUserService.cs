using Frozen.Application.DTOs.Users;

namespace Frozen.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto> CreateAdminAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task SetRolesAsync(Guid userId, IReadOnlyList<string> roles, CancellationToken cancellationToken = default);
}
