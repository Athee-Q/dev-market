namespace Order.Domain;

public class Order
{
    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = default!;
    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public Order(Guid customerId, IEnumerable<(Guid ProductId, int Quantity, decimal UnitPrice, string ProductName, string ProductType, string? AssetUrl)> items)
    {
        Id = Guid.NewGuid();
        OrderNumber = $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{Id.ToString("N")[..6].ToUpperInvariant()}";
        CustomerId = customerId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
        Status = OrderStatus.Pending;

        foreach (var (productId, quantity, unitPrice, productName, productType, assetUrl) in items)
            _items.Add(new OrderItem(Id, productId, quantity, unitPrice, productName, productType, assetUrl));

        TotalAmount = _items.Sum(i => i.TotalPrice);
    }

    /// <summary>Digital goods have no stock to reserve, so an order confirms right after it's created — see CreateOrder.Handler.</summary>
    public void Confirm()
    {
        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Payment captured and verified — the order's terminal success state. Each item's AccessKey becomes visible to the buyer from here on (see OrderDto).</summary>
    public void Complete()
    {
        Status = OrderStatus.Completed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// A payment attempt failed or its signature/webhook verification failed. Not terminal — the
    /// same Razorpay order can still accept another attempt (see Payment Service), so the
    /// customer can retry from here.
    /// </summary>
    public void MarkPaymentFailed()
    {
        Status = OrderStatus.PaymentFailed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
