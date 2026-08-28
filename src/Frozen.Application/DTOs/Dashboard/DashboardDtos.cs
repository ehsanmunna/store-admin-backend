using Frozen.Domain.Enums;

namespace Frozen.Application.DTOs.Dashboard;

public record DashboardSummaryDto(int TotalProducts, int TotalOrders, decimal TotalRevenue);

public record OrdersOverTimePointDto(DateOnly Date, int Count);

public record OrdersByStatusPointDto(OrderStatus Status, int Count);

public record DashboardDto(
    DashboardSummaryDto Summary,
    IReadOnlyList<OrdersOverTimePointDto> OrdersOverTime,
    IReadOnlyList<OrdersByStatusPointDto> OrdersByStatus);
