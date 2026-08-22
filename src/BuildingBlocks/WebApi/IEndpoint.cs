using Microsoft.AspNetCore.Routing;

namespace ECommerce.BuildingBlocks.WebApi;

/// <summary>
/// One vertical-slice endpoint — maps exactly one Minimal API route. Implementations are
/// discovered by assembly scan (see EndpointExtensions) instead of being wired up by hand in
/// Program.cs, so adding a new slice never means editing a shared file.
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
