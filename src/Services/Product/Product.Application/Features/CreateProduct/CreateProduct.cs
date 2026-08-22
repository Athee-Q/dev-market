using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using FluentValidation;
using Product.Application.Abstractions;
using Product.Application.Dto;
using Product.Domain;

namespace Product.Application.Features.CreateProduct;

public static class CreateProduct
{
    public record Command(
        Guid CategoryId, string Name, string Description, decimal Price, string SKU,
        ProductType ProductType, PricingModel PricingModel, string? AssetUrl) : IRequest<ProductDto>;

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

    public class Handler(IProductRepository repository) : IRequestHandler<Command, ProductDto>
    {
        public async Task<ProductDto> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await repository.SkuExistsAsync(request.SKU, null, cancellationToken))
                throw new ConflictException($"A product with SKU '{request.SKU}' already exists.");

            var product = new Domain.Product(
                request.CategoryId, request.Name, request.Description, request.Price, request.SKU,
                request.ProductType, request.PricingModel, request.AssetUrl);

            await repository.AddAsync(product, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            return ProductDto.FromDomain(product);
        }
    }
}
