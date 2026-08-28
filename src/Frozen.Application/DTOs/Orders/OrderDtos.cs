using Frozen.Domain.Enums;

namespace Frozen.Application.DTOs.Orders;

public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    OrderStatus Status,
    string ShippingFullName,
    string ShippingAddressLine1,
    string? ShippingAddressLine2,
    string ShippingCity,
    string? ShippingState,
    string ShippingPostalCode,
    string? ShippingCountry,
    string? ShippingPhone,
    decimal SubTotal,
    decimal ShippingCost,
    decimal Tax,
    decimal TotalAmount,
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> Items);

public record CreateOrderItemRequest(Guid ProductId, int Quantity);

public record CreateOrderRequest(
    string ShippingFullName,
    string ShippingAddressLine1,
    string? ShippingAddressLine2,
    string ShippingCity,
    string ShippingState,
    string ShippingPostalCode,
    string ShippingCountry,
    string? ShippingPhone,
    string? Notes,
    IReadOnlyList<CreateOrderItemRequest> Items);

public record UpdateOrderStatusRequest(OrderStatus Status);
