namespace Frozen.Application.DTOs.Messaging;

public record OrderCreatedEvent(
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    OrderEventAddress ShippingAddress,
    List<OrderEventItem> Items,
    decimal TotalAmount,
    DateTime CreatedAt);

public record OrderEventAddress(
    string FullName,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string PostalCode,
    string Phone);

public record OrderEventItem(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

public record OrderStatusUpdatedEvent(
    string OrderNumber,
    string Status);
