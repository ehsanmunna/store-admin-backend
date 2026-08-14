namespace Frozen.Application.DTOs.Auth;

public record RegisterRequest(string FirstName, string LastName, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, string Email, string FirstName, string LastName, IReadOnlyList<string> Roles);
