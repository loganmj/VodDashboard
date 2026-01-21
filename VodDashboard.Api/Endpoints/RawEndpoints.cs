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
                (RawFileService rawService, ILogger<RawFileService> logger) => 
                    GetRawFilesHandler(rawService, logger))
            .WithName("GetRawFiles")
            .WithOpenApi();

            return endpoints;
        }

        internal static IResult GetRawFilesHandler(RawFileService rawService, ILogger<RawFileService> logger)
        {
            try
            {
                var files = rawService.GetRawFiles();
                return Results.Ok(files);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Configuration error: {Message}", ex.Message);
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Configuration Error");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError(ex, "Access denied while retrieving raw files");
                return Results.Problem(
                    detail: "Access to the file system was denied.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Access Denied");
            }
            catch (IOException ex)
            {
                logger.LogError(ex, "I/O error while retrieving raw files");
                return Results.Problem(
                    detail: "A file system error occurred while retrieving raw files.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "File System Error");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while retrieving raw files");
                return Results.Problem(
                    detail: "An unexpected error occurred while retrieving raw files.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal Server Error");
            }
        }
    }
}
