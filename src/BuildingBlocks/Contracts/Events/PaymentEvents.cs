namespace ECommerce.Contracts.Events;

/// <summary>Published by Payment Service once a Razorpay payment is verified (signature or webhook) and captured.</summary>
public record PaymentSucceededEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string RazorpayOrderId,
    string RazorpayPaymentId,
    decimal Amount,
    DateTimeOffset PaidAt);

/// <summary>Published by Payment Service when a payment attempt fails or signature/webhook verification fails.</summary>
public record PaymentFailedEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string Reason,
    DateTimeOffset FailedAt);
