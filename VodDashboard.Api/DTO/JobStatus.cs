namespace VodDashboard.Api.DTO
{
    public record JobStatus(
        bool IsRunning,
        string? CurrentFile,
        string? Stage,
        int? Percent,
        DateTimeOffset? LastUpdated);
}
