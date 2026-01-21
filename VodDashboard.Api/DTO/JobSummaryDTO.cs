namespace VodDashboard.Api.DTO
{
    public record JobSummaryDTO(
        string Id,
        bool HasCleanVideo,
        int HighlightCount,
        int SceneCount,
        DateTimeOffset Created);
}
