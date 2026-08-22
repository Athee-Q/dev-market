namespace Product.Domain;

public class Product
{
    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public string SKU { get; private set; } = default!;
    public ProductType ProductType { get; private set; }
    public PricingModel PricingModel { get; private set; }

    /// <summary>Repo/download/docs link handed to the buyer on purchase — see Order Service's OrderItem.AssetUrl, snapshotted at order time so later edits here don't change what a past buyer received.</summary>
    public string? AssetUrl { get; private set; }

    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Product() { }

    public Product(
        Guid categoryId, string name, string description, decimal price, string sku,
        ProductType productType, PricingModel pricingModel, string? assetUrl)
    {
        Id = Guid.NewGuid();
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        SKU = sku;
        ProductType = productType;
        PricingModel = pricingModel;
        AssetUrl = assetUrl;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void Update(
        Guid categoryId, string name, string description, decimal price, string sku, bool isActive,
        ProductType productType, PricingModel pricingModel, string? assetUrl)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        SKU = sku;
        IsActive = isActive;
        ProductType = productType;
        PricingModel = pricingModel;
        AssetUrl = assetUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
