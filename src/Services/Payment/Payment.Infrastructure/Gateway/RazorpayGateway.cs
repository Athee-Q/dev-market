using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Payment.Application.Abstractions;
using Razorpay.Api;

namespace Payment.Infrastructure.Gateway;

/// <summary>
/// Wraps the official Razorpay .NET SDK (Razorpay.Api) for Orders/signature verification — every
/// SDK call is synchronous, so it's offloaded via Task.Run so it doesn't block the ASP.NET Core
/// request pipeline. The UPI QR Code API (below) has no SDK support at all, so those two methods
/// talk to Razorpay's REST API directly over a plain HttpClient with HTTP Basic auth instead.
/// </summary>
public class RazorpayGateway : IPaymentGateway
{
    private readonly RazorpayClient _client;
    private readonly HttpClient _httpClient;
    private readonly string _webhookSecret;

    public string KeyId { get; }

    public RazorpayGateway(IConfiguration configuration)
    {
        KeyId = configuration["Razorpay:KeyId"]
            ?? throw new InvalidOperationException("Configuration 'Razorpay:KeyId' is not set.");
        var keySecret = configuration["Razorpay:KeySecret"]
            ?? throw new InvalidOperationException("Configuration 'Razorpay:KeySecret' is not set.");

        // Optional: only the webhook endpoint needs it, and local dev without a public URL never
        // receives webhook calls at all — see README for how to enable it via a tunnel.
        _webhookSecret = configuration["Razorpay:WebhookSecret"] ?? string.Empty;

        _client = new RazorpayClient(KeyId, keySecret);

        _httpClient = new HttpClient { BaseAddress = new Uri("https://api.razorpay.com/v1/") };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{KeyId}:{keySecret}")));
    }

    public Task<string> CreateOrderAsync(Guid orderId, decimal amount, string currency, CancellationToken ct) =>
        Task.Run(() =>
        {
            var input = new Dictionary<string, object>
            {
                // Razorpay amounts are in the smallest currency unit — paise for INR.
                ["amount"] = (int)Math.Round(amount * 100m, MidpointRounding.AwayFromZero),
                ["currency"] = currency,
                ["receipt"] = orderId.ToString(),
                ["payment_capture"] = 1,
            };

            var order = _client.Order.Create(input);

            // The SDK's Entity indexer returns dynamic — assign to a statically-typed local so
            // Task.Run<TResult> infers Task<string> instead of Task<dynamic> for this lambda.
            string razorpayOrderId = order["id"].ToString()!;
            return razorpayOrderId;
        }, ct);

    public bool VerifyCheckoutSignature(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
    {
        try
        {
            Utils.verifyPaymentSignature(new Dictionary<string, string>
            {
                ["razorpay_order_id"] = razorpayOrderId,
                ["razorpay_payment_id"] = razorpayPaymentId,
                ["razorpay_signature"] = razorpaySignature,
            });
            return true;
        }
        catch (Exception)
        {
            // The SDK signals a bad signature by throwing rather than returning false — this
            // adapter is the boundary that converts that into the bool IPaymentGateway exposes.
            return false;
        }
    }

    public bool VerifyWebhookSignature(string rawBody, string signatureHeader)
    {
        if (string.IsNullOrEmpty(_webhookSecret) || string.IsNullOrEmpty(signatureHeader))
            return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
        var expected = Convert.ToHexStringLower(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody)));

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signatureHeader.ToLowerInvariant()));
    }

    public async Task<(string QrCodeId, string ImageUrl, DateTimeOffset ExpiresAt)> CreateUpiQrCodeAsync(
        Guid orderId, decimal amount, CancellationToken ct)
    {
        var closeBy = DateTimeOffset.UtcNow.AddMinutes(15);
        var payload = new
        {
            type = "upi_qr",
            name = "DevMarket order payment",
            usage = "single_use",
            fixed_amount = true,
            payment_amount = (int)Math.Round(amount * 100m, MidpointRounding.AwayFromZero),
            description = $"Order {orderId}",
            close_by = closeBy.ToUnixTimeSeconds(),
            notes = new Dictionary<string, string> { ["orderId"] = orderId.ToString() },
        };

        using var response = await _httpClient.PostAsJsonAsync("payments/qr_codes", payload, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<QrCodeResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Razorpay returned an empty QR code response.");

        return (body.Id, body.ImageUrl, DateTimeOffset.FromUnixTimeSeconds(body.CloseBy));
    }

    public async Task<string?> GetQrCodePaymentIdAsync(string qrCodeId, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync($"payments/qr_codes/{qrCodeId}/payments", ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<QrCodePaymentsResponse>(cancellationToken: ct);
        return body?.Items?.FirstOrDefault(item => item.Status is "captured" or "authorized")?.Id;
    }

    private record QrCodeResponse(
        string Id,
        [property: JsonPropertyName("image_url")] string ImageUrl,
        [property: JsonPropertyName("close_by")] long CloseBy);

    private record QrCodePaymentsResponse([property: JsonPropertyName("items")] List<QrCodePaymentItem> Items);

    private record QrCodePaymentItem(string Id, string Status);
}
