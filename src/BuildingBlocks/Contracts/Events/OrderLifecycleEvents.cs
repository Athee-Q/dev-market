namespace ECommerce.Contracts.Events;

/// <summary>
/// Published by Order Service after inventory reservation succeeds and the order is confirmed.
/// Carries TotalAmount so Payment Service can create a gateway order without calling back into
/// Order Service for it.
/// </summary>
public record OrderConfirmedEvent(
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    decimal TotalAmount,
    DateTimeOffset ConfirmedAt);

/// <summary>Published by Order Service when an order is cancelled (reservation failure or explicit cancel).</summary>
public record OrderCancelledEvent(
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string Reason,
    DateTimeOffset CancelledAt);
