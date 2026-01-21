using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Endpoints
{
    public static class RawEndpoints
    {
        public static IEndpointRouteBuilder MapRawEndpoints(this IEndpointRouteBuilder endpoints)
        {
            RouteGroupBuilder group = endpoints.MapGroup("/api/raw");

            group.MapGet(
                string.Empty,
                async (RawFileService rawService, CancellationToken cancellationToken) =>
                {
                    var files = await rawService.GetRawFiles(cancellationToken);
                    return Results.Ok(files);
                })
            .WithName("GetRawFiles");

            return endpoints;
        }
    }
}
