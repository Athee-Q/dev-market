using System.Globalization;
using ECommerce.Grpc.ProductCatalog;
using Grpc.Core;

namespace Cart.Api.ExternalServices;

/// <summary>
/// gRPC-backed IProductCatalogClient — replaces the old HTTP/JSON implementation. Adapts the
/// generated ProductCatalog.ProductCatalogClient (see Product.Api/Protos/product.proto) to Cart's
/// own IProductCatalogClient/ProductInfo shape, so CachedProductCatalogClient and everything above
/// it needs no changes for the transport swap.
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
                Guid.Parse(reply.Id), reply.Name, decimal.Parse(reply.Price, CultureInfo.InvariantCulture), reply.IsActive);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
