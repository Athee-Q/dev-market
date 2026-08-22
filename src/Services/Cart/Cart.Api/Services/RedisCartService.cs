using System.Text.Json;
using Cart.Api.ExternalServices;
using Cart.Api.Models;
using ECommerce.Contracts.Common;
using StackExchange.Redis;

namespace Cart.Api.Services;

/// <summary>
/// Cart stored as a single JSON document under "cart:{customerId}" with a sliding TTL, per §11
/// ("Cart can use a TTL to remove abandoned carts"). A production version could split into a
/// hash (cart:{customerId}) + per-item fields (cart:{customerId}:items) for partial updates
/// without a read-modify-write round trip — kept as one document here for simplicity.
/// </summary>
public class RedisCartService(IConnectionMultiplexer redis, IProductCatalogClient productCatalogClient) : ICartService
{
    private static readonly TimeSpan CartTtl = TimeSpan.FromDays(7);
    private IDatabase Db => redis.GetDatabase();
    private static string Key(Guid customerId) => $"cart:{customerId}";

    public async Task<CartDto> GetCartAsync(Guid customerId, CancellationToken ct)
    {
        var value = await Db.StringGetAsync(Key(customerId));
        return value.HasValue
            ? JsonSerializer.Deserialize<CartDto>((string)value!)!
            : new CartDto(customerId, []);
    }

    public async Task<CartDto> AddItemAsync(Guid customerId, AddCartItemRequest request, CancellationToken ct)
    {
        if (request.Quantity <= 0)
            throw new ValidationAppException("Quantity must be greater than zero.");

        var product = await productCatalogClient.GetProductAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Product '{request.ProductId}' does not exist.");

        var cart = await GetCartAsync(customerId, ct);
        var items = cart.Items.ToDictionary(i => i.ProductId);

        items[request.ProductId] = items.TryGetValue(request.ProductId, out var existing)
            ? existing with { Quantity = existing.Quantity + request.Quantity, UnitPrice = product.Price, ProductName = product.Name }
            : new CartItemDto(product.Id, product.Name, product.Price, request.Quantity);

        return await SaveAsync(customerId, items.Values.ToList());
    }

    public async Task<CartDto?> UpdateItemAsync(Guid customerId, Guid productId, UpdateCartItemRequest request, CancellationToken ct)
    {
        var cart = await GetCartAsync(customerId, ct);
        var items = cart.Items.ToDictionary(i => i.ProductId);
        if (!items.TryGetValue(productId, out var existing)) return null;

        if (request.Quantity <= 0)
            items.Remove(productId);
        else
            items[productId] = existing with { Quantity = request.Quantity };

        return await SaveAsync(customerId, items.Values.ToList());
    }

    public async Task<CartDto> RemoveItemAsync(Guid customerId, Guid productId, CancellationToken ct)
    {
        var cart = await GetCartAsync(customerId, ct);
        var items = cart.Items.Where(i => i.ProductId != productId).ToList();
        return await SaveAsync(customerId, items);
    }

    private async Task<CartDto> SaveAsync(Guid customerId, IReadOnlyCollection<CartItemDto> items)
    {
        var cart = new CartDto(customerId, items);
        await Db.StringSetAsync(Key(customerId), JsonSerializer.Serialize(cart), CartTtl);
        return cart;
    }
}
