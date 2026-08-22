using System.Globalization;
using ECommerce.Grpc.ProductCatalog;
using Grpc.Core;
using Order.Application.Abstractions;

namespace Order.Infrastructure.ExternalServices;

/// <summary>
/// gRPC-backed IProductCatalogClient — replaces the old HTTP/JSON implementation (§5: cross-service
/// data via REST or events, never a shared database — gRPC is still a direct service call, just a
/// different transport). Adapts the generated ProductCatalog.ProductCatalogClient (see
/// Product.Api/Protos/product.proto) to Order's own IProductCatalogClient/ProductInfo shape, so
/// CreateOrder.Handler needs no changes for the transport swap.
/// </summary>
public class GrpcProductCatalogClient(ProductCatalog.ProductCatalogClient client) : IProductCatalogClient
{
    public async Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken ct)
    {
        try
        {
            var reply = await client.GetProductAsync(
                new GetProductRequest { ProductId = productId.ToString() },
                cancellationToken: ct);

            return new ProductInfo(
                Guid.Parse(reply.Id), reply.Name, decimal.Parse(reply.Price, CultureInfo.InvariantCulture), reply.IsActive,
                reply.ProductType, reply.HasAssetUrl ? reply.AssetUrl : null);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
