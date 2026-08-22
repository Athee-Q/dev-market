namespace Payment.Application.Abstractions;

public interface IPaymentRepository
{
    Task<Domain.Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct);
    Task<Domain.Payment?> GetByRazorpayOrderIdAsync(string razorpayOrderId, CancellationToken ct);

    Task<(IReadOnlyCollection<Domain.Payment> Items, int TotalCount)> SearchAsync(
        Guid? customerId, Domain.PaymentStatus? status, int page, int pageSize, CancellationToken ct);

    /// <summary>Sum + count of every Succeeded payment — backs the Admin dashboard's revenue tile.</summary>
    Task<(decimal TotalRevenue, int SucceededCount)> GetRevenueSummaryAsync(CancellationToken ct);

    Task AddAsync(Domain.Payment payment, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

/// <summary>Abstraction over the message bus so Application stays free of a MassTransit dependency.</summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken ct) where T : class;
}

/// <summary>Abstraction over the Razorpay SDK so Application stays free of a payment-gateway dependency.</summary>
public interface IPaymentGateway
{
    /// <summary>The publishable Razorpay key id, safe to hand to the frontend to open Checkout.</summary>
    string KeyId { get; }

    /// <summary>Creates a Razorpay order for the given internal order and returns its Razorpay order id.</summary>
    Task<string> CreateOrderAsync(Guid orderId, decimal amount, string currency, CancellationToken ct);

    /// <summary>Verifies the HMAC signature Razorpay Checkout returns to the browser on payment success.</summary>
    bool VerifyCheckoutSignature(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);

    /// <summary>Verifies the HMAC signature on an incoming Razorpay webhook call against its raw request body.</summary>
    bool VerifyWebhookSignature(string rawBody, string signatureHeader);

    /// <summary>Creates a single-use, fixed-amount UPI QR code for an order — see CreateUpiQr.</summary>
    Task<(string QrCodeId, string ImageUrl, DateTimeOffset ExpiresAt)> CreateUpiQrCodeAsync(Guid orderId, decimal amount, CancellationToken ct);

    /// <summary>Returns the Razorpay payment id if the given QR code has a captured/authorized payment against it yet, else null — see CheckUpiQrPayment.</summary>
    Task<string?> GetQrCodePaymentIdAsync(string qrCodeId, CancellationToken ct);
}
