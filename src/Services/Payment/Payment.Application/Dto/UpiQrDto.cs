namespace Payment.Application.Dto;

/// <summary>What the frontend needs to render a scannable UPI QR and know when to stop polling.</summary>
public record UpiQrDto(string QrCodeId, string ImageUrl, DateTimeOffset ExpiresAt);
