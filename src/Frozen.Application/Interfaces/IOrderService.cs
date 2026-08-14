using Frozen.Application.Common;
using Frozen.Application.DTOs.Orders;
using Frozen.Domain.Enums;

namespace Frozen.Application.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderDto>> GetAllAsync(PagedRequest request, Guid? userId, OrderStatus? status, CancellationToken cancellationToken = default);
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
}
