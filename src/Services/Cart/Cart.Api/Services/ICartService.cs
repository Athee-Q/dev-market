using Cart.Api.Models;

namespace Cart.Api.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid customerId, CancellationToken ct);
    Task<CartDto> AddItemAsync(Guid customerId, AddCartItemRequest request, CancellationToken ct);
    Task<CartDto?> UpdateItemAsync(Guid customerId, Guid productId, UpdateCartItemRequest request, CancellationToken ct);
    Task<CartDto> RemoveItemAsync(Guid customerId, Guid productId, CancellationToken ct);
}
