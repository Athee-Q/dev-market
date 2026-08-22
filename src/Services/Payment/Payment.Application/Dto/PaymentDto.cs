using Payment.Domain;

namespace Payment.Application.Dto;

/// <summary>
/// Includes RazorpayKeyId (the publishable key, safe for the browser) alongside RazorpayOrderId
/// and Amount — everything the frontend needs to open Razorpay Checkout in one response.
/// </summary>
public record PaymentDto(
    Guid Id,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    string RazorpayKeyId,
    string RazorpayOrderId,
    string? RazorpayPaymentId,
    string? FailureReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static PaymentDto FromDomain(Domain.Payment p, string razorpayKeyId) => new(
        p.Id, p.OrderId, p.CustomerId, p.OrderNumber, p.Amount, p.Currency, p.Status,
        razorpayKeyId, p.RazorpayOrderId, p.RazorpayPaymentId, p.FailureReason, p.CreatedAt, p.UpdatedAt);
}
