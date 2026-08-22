namespace Order.Domain;

public enum OrderStatus
{
    /// <summary>Saved, about to be confirmed — momentary; there's no stock to reserve for a digital catalog, so CreateOrder confirms immediately after this.</summary>
    Pending,
    Confirmed,
    PaymentFailed,
    Cancelled,
    Completed,
}
