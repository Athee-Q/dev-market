using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using FluentValidation;
using Product.Application.Abstractions;
using Product.Application.Dto;
using Product.Domain;

namespace Product.Application.Features.UpdateProduct;

public static class UpdateProduct
{
    public record Command(
        Guid Id, Guid CategoryId, string Name, string Description, decimal Price, string SKU, bool IsActive,
        ProductType ProductType, PricingModel PricingModel, string? AssetUrl) : IRequest<ProductDto?>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotNull().MaximumLength(2000);
            RuleFor(x => x.SKU).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.CategoryId).NotEmpty();
            RuleFor(x => x.ProductType).IsInEnum();
            RuleFor(x => x.PricingModel).IsInEnum();
            RuleFor(x => x.AssetUrl).MaximumLength(2000);
        }
    }

    public class Handler(IProductRepository repository, IProductCacheInvalidator cacheInvalidator) : IRequestHandler<Command, ProductDto?>
    {
        public async Task<ProductDto?> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null) return null;

            if (await repository.SkuExistsAsync(request.SKU, request.Id, cancellationToken))
                throw new ConflictException($"A product with SKU '{request.SKU}' already exists.");

            product.Update(
                request.CategoryId, request.Name, request.Description, request.Price, request.SKU, request.IsActive,
                request.ProductType, request.PricingModel, request.AssetUrl);
            await repository.SaveChangesAsync(cancellationToken);

            // Invalidate rather than update in place — GetProductById re-populates the cache on next read.
            // Evicts Redis (L2) and, via pub/sub, every instance's local in-memory cache (L1).
            await cacheInvalidator.InvalidateAsync(request.Id, cancellationToken);

            return ProductDto.FromDomain(product);
        }
    }
}
