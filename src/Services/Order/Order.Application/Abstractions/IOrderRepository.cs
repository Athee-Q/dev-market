namespace Order.Application.Abstractions;

public interface IOrderRepository
{
    Task<Domain.Order?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<(IReadOnlyCollection<Domain.Order> Items, int TotalCount)> SearchAsync(
        Guid? customerId, Domain.OrderStatus? status, int page, int pageSize, CancellationToken ct);

    Task AddAsync(Domain.Order order, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

/// <summary>Read-only client for Product Service's synchronous REST API (price/availability lookups at order time).</summary>
public interface IProductCatalogClient
{
    Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken ct);
}

public record ProductInfo(Guid Id, string Name, decimal Price, bool IsActive, string ProductType, string? AssetUrl);

/// <summary>Abstraction over the message bus so Application stays free of a MassTransit dependency.</summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken ct) where T : class;
}
