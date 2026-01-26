namespace VodDashboard.Api.DTO
{
    public interface IJobStatus
    {
        public bool IsRunning { get; }
        public string? JobId { get; }
        public string? FileName { get; }
        public string? CurrentFile { get; }
        public string? Stage { get; }
        public int? Percent { get; }
        public DateTime? Timestamp { get; }
        public DateTime? LastUpdated { get; }
        public TimeSpan? EstimatedTimeRemaining { get; }
        public TimeSpan? ElapsedTime { get; }
    }

    public record JobStatus(
        bool IsRunning,
        string? JobId,
        string? FileName,
        string? CurrentFile,
        string? Stage,
        int? Percent,
        DateTime? Timestamp,
        DateTime? LastUpdated,
        TimeSpan? EstimatedTimeRemaining,
        TimeSpan? ElapsedTime) : IJobStatus;
}
