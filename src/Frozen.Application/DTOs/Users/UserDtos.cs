namespace Frozen.Application.DTOs.Users;

public record UserDto(Guid Id, string FirstName, string LastName, string Email, IReadOnlyList<string> Roles);

public record CreateUserRequest(string FirstName, string LastName, string Email, string Password, IReadOnlyList<string>? Roles = null);

public record ResetPasswordRequest(string NewPassword);
