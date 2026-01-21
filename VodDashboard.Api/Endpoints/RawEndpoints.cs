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
                    try
                    {
                        var files = rawService.GetRawFiles();
                        return Results.Ok(files);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.Problem(
                            detail: ex.Message,
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Configuration Error");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        return Results.Problem(
                            detail: ex.Message,
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Access Denied");
                    }
                    catch (IOException ex)
                    {
                        return Results.Problem(
                            detail: ex.Message,
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "File System Error");
                    }
                    catch (Exception)
                    {
                        return Results.Problem(
                            detail: "An unexpected error occurred while retrieving raw files.",
                            statusCode: StatusCodes.Status500InternalServerError,
                            title: "Internal Server Error");
                    }
                })
            .WithName("GetRawFiles");

            return endpoints;
        }
    }
}
