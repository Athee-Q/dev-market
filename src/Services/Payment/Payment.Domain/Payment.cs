namespace Payment.Domain;

/// <summary>
/// One row per order's payment. Keyed uniquely by OrderId (see PaymentConfiguration) so the
/// OrderConfirmedEvent consumer is idempotent — a redelivered event just finds the row already
/// there. A single Razorpay order can accept more than one payment attempt, so a Failed
/// payment here is retried in place — RazorpayPaymentId and FailureReason are simply overwritten
/// by the next attempt rather than creating a new row.
/// </summary>
public class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string OrderNumber { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public PaymentStatus Status { get; private set; }
    public string RazorpayOrderId { get; private set; } = default!;
    public string? RazorpayPaymentId { get; private set; }
    public string? FailureReason { get; private set; }

    // UPI QR flow (CreateUpiQr/CheckUpiQrPayment) — a separate, headless alternative to opening
    // Razorpay Checkout: the frontend shows this image, scanned with any UPI app, and polls
    // status instead of getting a client-side handler callback the way Checkout provides one.
    public string? RazorpayQrCodeId { get; private set; }
    public string? UpiQrImageUrl { get; private set; }
    public DateTimeOffset? UpiQrExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Payment() { }

    public Payment(Guid orderId, Guid customerId, string orderNumber, decimal amount, string currency, string razorpayOrderId)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId;
        OrderNumber = orderNumber;
        Amount = amount;
        Currency = currency;
        Status = PaymentStatus.Created;
        RazorpayOrderId = razorpayOrderId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void MarkSucceeded(string razorpayPaymentId)
    {
        Status = PaymentStatus.Succeeded;
        RazorpayPaymentId = razorpayPaymentId;
        FailureReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AttachUpiQr(string qrCodeId, string imageUrl, DateTimeOffset expiresAt)
    {
        RazorpayQrCodeId = qrCodeId;
        UpiQrImageUrl = imageUrl;
        UpiQrExpiresAt = expiresAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
