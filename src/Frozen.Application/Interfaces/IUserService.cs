using Frozen.Application.DTOs.Users;

namespace Frozen.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto> CreateAdminAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
}
