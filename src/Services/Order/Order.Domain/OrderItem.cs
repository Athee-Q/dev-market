using System.Security.Cryptography;

namespace Order.Domain;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }

    // Snapshotted from Product Service at order time (see IProductCatalogClient) so a later edit
    // or deletion of the product never changes what a past buyer sees/received.
    public string ProductName { get; private set; } = default!;
    public string ProductType { get; private set; } = default!;
    public string? AssetUrl { get; private set; }

    /// <summary>The credential handed to the buyer for this line item — generated at order time, only ever surfaced to the buyer once the order is Completed (see OrderDto).</summary>
    public string AccessKey { get; private set; } = default!;

    private OrderItem() { }

    public OrderItem(Guid orderId, Guid productId, int quantity, decimal unitPrice, string productName, string productType, string? assetUrl)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = quantity * unitPrice;
        ProductName = productName;
        ProductType = productType;
        AssetUrl = assetUrl;
        AccessKey = GenerateAccessKey();
    }

    /// <summary>A Stripe-style opaque credential — not a real cryptographic API key for a live service, just a unique delivered value (see README "Known gaps": this is a marketplace for keys, not an API gateway product).</summary>
    private static string GenerateAccessKey()
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var bytes = RandomNumberGenerator.GetBytes(32);
        var chars = new char[32];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = alphabet[bytes[i] % alphabet.Length];

        return $"sk_live_{new string(chars)}";
    }
}
