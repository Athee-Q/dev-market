using Product.Domain;

namespace Product.Application.Dto;

public record ProductDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    string SKU,
    ProductType ProductType,
    PricingModel PricingModel,
    string? AssetUrl,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static ProductDto FromDomain(Domain.Product p) => new(
        p.Id, p.CategoryId, p.Name, p.Description, p.Price, p.SKU,
        p.ProductType, p.PricingModel, p.AssetUrl, p.IsActive, p.CreatedAt, p.UpdatedAt);
}
