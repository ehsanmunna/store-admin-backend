using Frozen.Application.Common;
using Frozen.Application.DTOs.Orders;
using Frozen.Application.DTOs.Messaging;
using Frozen.Application.Exceptions;
using Frozen.Application.Interfaces;
using Frozen.Domain.Entities;
using Frozen.Domain.Enums;
using Frozen.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Frozen.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderEventPublisher? _orderEventPublisher;

    public OrderService(IUnitOfWork unitOfWork, IOrderEventPublisher? orderEventPublisher = null)
    {
        _unitOfWork = unitOfWork;
        _orderEventPublisher = orderEventPublisher;
    }

    public async Task<PagedResult<OrderDto>> GetAllAsync(PagedRequest request, Guid? userId, OrderStatus? status, CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = _unitOfWork.Orders.Query().Include(o => o.Items);

        if (userId.HasValue)
            query = query.Where(o => o.UserId == userId.Value);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(o => o.OrderNumber.Contains(request.Search));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.Query()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        return ToDto(order);
    }

    public async Task<OrderDto> CreateAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
            throw new ValidationException("Order must contain at least one item.");

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            Status = OrderStatus.Pending,
            ShippingFullName = request.ShippingFullName,
            ShippingAddressLine1 = request.ShippingAddressLine1,
            ShippingAddressLine2 = request.ShippingAddressLine2,
            ShippingCity = request.ShippingCity,
            ShippingState = request.ShippingState,
            ShippingPostalCode = request.ShippingPostalCode,
            ShippingCountry = request.ShippingCountry,
            ShippingPhone = request.ShippingPhone,
            Notes = request.Notes
        };

        decimal subTotal = 0;

        foreach (var itemRequest in request.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(itemRequest.ProductId, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), itemRequest.ProductId);

            if (product.StockQuantity < itemRequest.Quantity)
                throw new ValidationException($"Insufficient stock for product '{product.Name}'.");

            product.StockQuantity -= itemRequest.Quantity;
            _unitOfWork.Products.Update(product);

            var lineTotal = product.Price * itemRequest.Quantity;
            subTotal += lineTotal;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.Price
            });
        }

        order.SubTotal = subTotal;
        order.ShippingCost = 0;
        order.Tax = 0;
        order.TotalAmount = subTotal + order.ShippingCost + order.Tax;

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(order.Id, cancellationToken);
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_orderEventPublisher is not null)
        {
            await _orderEventPublisher.PublishOrderStatusUpdatedAsync(order.OrderNumber, request.Status, cancellationToken);
        }

        return await GetByIdAsync(order.Id, cancellationToken);
    }

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

    private static OrderDto ToDto(Order o) => new(
        o.Id, o.OrderNumber, o.UserId, o.Status,
        o.ShippingFullName, o.ShippingAddressLine1, o.ShippingAddressLine2, o.ShippingCity,
        o.ShippingState, o.ShippingPostalCode, o.ShippingCountry, o.ShippingPhone,
        o.SubTotal, o.ShippingCost, o.Tax, o.TotalAmount, o.Notes, o.CreatedAt,
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.LineTotal)).ToList());
}
