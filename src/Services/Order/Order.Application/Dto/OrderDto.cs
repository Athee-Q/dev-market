using Order.Domain;

namespace Order.Application.Dto;

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string ProductName,
    string ProductType,
    string? AssetUrl,
    string? AccessKey);

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    decimal TotalAmount,
    OrderStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<OrderItemDto> Items)
{
    public static OrderDto FromDomain(Domain.Order o) => new(
        o.Id, o.OrderNumber, o.CustomerId, o.TotalAmount, o.Status, o.CreatedAt, o.UpdatedAt,
        o.Items.Select(i => new OrderItemDto(
            i.Id, i.ProductId, i.Quantity, i.UnitPrice, i.TotalPrice, i.ProductName, i.ProductType, i.AssetUrl,
            // Only reveal the access key once payment has actually gone through — an unpaid/pending
            // order shouldn't leak the credential it would eventually deliver.
            o.Status == OrderStatus.Completed ? i.AccessKey : null)).ToList());
}
