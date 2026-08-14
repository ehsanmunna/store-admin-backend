using System.Security.Claims;
using Frozen.Application.Common;
using Frozen.Application.DTOs.Orders;
using Frozen.Application.Interfaces;
using Frozen.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frozen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetAll(
        [FromQuery] PagedRequest request,
        [FromQuery] OrderStatus? status,
        CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(UserRoles.Admin);
        var userId = isAdmin ? (Guid?)null : GetUserId();

        return Ok(await _orderService.GetAllAsync(request, userId, status, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _orderService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateAsync(GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _orderService.UpdateStatusAsync(id, request, cancellationToken));
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User id claim not found.");
        return Guid.Parse(value);
    }
}
