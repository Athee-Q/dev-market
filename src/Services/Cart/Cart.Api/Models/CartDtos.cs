namespace Cart.Api.Models;

public record CartItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity)
{
    public decimal TotalPrice => UnitPrice * Quantity;
}

public record CartDto(Guid CustomerId, IReadOnlyCollection<CartItemDto> Items)
{
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
}

public record AddCartItemRequest(Guid ProductId, int Quantity);

public record UpdateCartItemRequest(int Quantity);
