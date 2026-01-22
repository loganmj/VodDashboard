namespace VodDashboard.Api.DTO
{
    public record JobDetailDto(
        string Id,
        bool HasCleanVideo,
        IReadOnlyList<string> Highlights,
        IReadOnlyList<string> Scenes,
        DateTimeOffset Created);
}
