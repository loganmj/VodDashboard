namespace VodDashboard.Api.DTO
{
    public record StatusDTO(
        bool IsRunning,
        string? CurrentFile,
        string? Stage,
        int? Percent,
        DateTimeOffset? LastUpdated);
}
