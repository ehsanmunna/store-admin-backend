namespace Frozen.Application.DTOs.Roles;

public record RoleDto(Guid Id, string Name, int UserCount);

public record CreateRoleRequest(string Name);

public record UpdateRoleRequest(string Name);

public record SetUserRolesRequest(IReadOnlyList<string> Roles);