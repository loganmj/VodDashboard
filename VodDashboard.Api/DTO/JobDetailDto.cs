namespace VodDashboard.Api.DTO
{
    public record JobDetailDto(
        string Id,
        bool HasCleanVideo,
        List<string> Highlights,
        List<string> Scenes,
        DateTimeOffset Created);
}
