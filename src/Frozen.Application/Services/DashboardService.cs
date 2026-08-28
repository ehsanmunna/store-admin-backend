using Frozen.Application.DTOs.Dashboard;
using Frozen.Application.Interfaces;
using Frozen.Domain.Enums;
using Frozen.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Frozen.Application.Services;

public class DashboardService : IDashboardService
{
    private const int OrdersOverTimeWindowDays = 14;
    private static readonly OrderStatus[] RevenueExcludedStatuses = [OrderStatus.Cancelled, OrderStatus.Refunded];

    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var products = _unitOfWork.Products.Query();
        var orders = _unitOfWork.Orders.Query();

        var totalProducts = await products.CountAsync(cancellationToken);
        var totalOrders = await orders.CountAsync(cancellationToken);
        var totalRevenue = await orders
            .Where(o => !RevenueExcludedStatuses.Contains(o.Status))
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        var summary = new DashboardSummaryDto(totalProducts, totalOrders, totalRevenue);
        var ordersOverTime = await GetOrdersOverTimeAsync(orders, cancellationToken);
        var ordersByStatus = await GetOrdersByStatusAsync(orders, cancellationToken);

        return new DashboardDto(summary, ordersOverTime, ordersByStatus);
    }

    private static async Task<IReadOnlyList<OrdersOverTimePointDto>> GetOrdersOverTimeAsync(
        IQueryable<Domain.Entities.Order> orders, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = today.AddDays(-(OrdersOverTimeWindowDays - 1));
        var since = DateTime.SpecifyKind(startDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        var counts = await orders
            .Where(o => o.CreatedAt >= since)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var countsByDate = counts.ToDictionary(c => DateOnly.FromDateTime(c.Date), c => c.Count);

        var series = new List<OrdersOverTimePointDto>(OrdersOverTimeWindowDays);
        for (var date = startDate; date <= today; date = date.AddDays(1))
        {
            series.Add(new OrdersOverTimePointDto(date, countsByDate.GetValueOrDefault(date)));
        }

        return series;
    }

    private static async Task<IReadOnlyList<OrdersByStatusPointDto>> GetOrdersByStatusAsync(
        IQueryable<Domain.Entities.Order> orders, CancellationToken cancellationToken)
    {
        return await orders
            .GroupBy(o => o.Status)
            .Select(g => new OrdersByStatusPointDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);
    }
}
