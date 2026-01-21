using VodDashboard.Api.Services;

namespace VodDashboard.Api.Endpoints
{
    /// <summary>
    /// Maps the raw endpoint
    /// </summary>
    public static class RawEndpoints
    {
        public static IEndpointRouteBuilder MapRawEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/api/raw");

            group.MapGet(
                string.Empty,
                (RawFileService rawService) =>
                {
                    var files = rawService.GetRawFiles();
                    return Results.Ok(files);
                })
            .WithName("GetRawFiles")
            .WithOpenApi();

            return endpoints;
        }
    }
}
