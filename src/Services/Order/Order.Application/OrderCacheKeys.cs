namespace Order.Application;

/// <summary>
/// Single source of truth for the order-by-id cache key, shared by GetOrderById's read-through
/// and every write handler that must invalidate it (CancelOrder, CompleteOrderAfterPayment,
/// MarkOrderPaymentFailed) — keeps the key string from drifting across those call sites.
/// </summary>
internal static class OrderCacheKeys
{
    public static string Id(Guid orderId) => $"order:{orderId}";
}
