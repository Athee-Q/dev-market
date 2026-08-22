using System.Globalization;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Grpc.ProductCatalog;
using Grpc.Core;
using Product.Application.Features.GetProductById;

namespace Product.Api.Services;

/// <summary>
/// gRPC counterpart to GetProductByIdEndpoint (REST) — same query, same mediator pipeline (so it
/// gets the same L1/L2 hybrid cache — see HybridCachingBehaviour/GetProductById.Query), just a
/// different transport for internal callers (Cart, Order). No authorization, same as the REST
/// endpoint: product catalog data is public, and the trust boundary is the Docker network itself.
/// </summary>
public class ProductCatalogGrpcService(IMediator mediator) : ProductCatalog.ProductCatalogBase
{
    public override async Task<ProductReply> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"'{request.ProductId}' is not a valid product id."));

        var product = await mediator.Send(new GetProductById.Query(productId), context.CancellationToken);
        if (product is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Product '{productId}' does not exist."));

        var reply = new ProductReply
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Price = product.Price.ToString(CultureInfo.InvariantCulture),
            IsActive = product.IsActive,
            ProductType = product.ProductType.ToString(),
        };
        if (product.AssetUrl is not null)
            reply.AssetUrl = product.AssetUrl;

        return reply;
    }
}
