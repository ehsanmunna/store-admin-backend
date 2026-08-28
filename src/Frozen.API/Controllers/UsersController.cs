using Frozen.Application.DTOs.Users;
using Frozen.Application.DTOs.Roles;
using Frozen.Application.Interfaces;
using Frozen.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frozen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Admin)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateAdminAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _userService.ResetPasswordAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> SetRoles(Guid id, SetUserRolesRequest request, CancellationToken cancellationToken)
    {
        await _userService.SetRolesAsync(id, request.Roles, cancellationToken);
        return NoContent();
    }
}
