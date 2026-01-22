namespace VodDashboard.Api.DTO
{
    public record JobData(
        string Id,
        bool HasCleanVideo,
        int HighlightCount,
        int SceneCount,
        DateTimeOffset Created);
}
