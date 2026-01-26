namespace VodDashboard.Api.DTO
{
    /// <summary>
    /// Interface for job status information.
    /// </summary>
    public interface IJobStatus
    {
        /// <summary>
        /// Gets a value indicating whether the job is currently running.
        /// </summary>
        public bool IsRunning { get; }
        
        /// <summary>
        /// Gets the unique identifier for the job.
        /// </summary>
        public string? JobId { get; }
        
        /// <summary>
        /// Gets the name of the file being processed.
        /// </summary>
        public string? FileName { get; }
        
        /// <summary>
        /// Gets the current file being worked on (may differ from FileName in multi-file scenarios).
        /// </summary>
        public string? CurrentFile { get; }
        
        /// <summary>
        /// Gets the current processing stage.
        /// </summary>
        public string? Stage { get; }
        
        /// <summary>
        /// Gets the completion percentage (0-100).
        /// </summary>
        public int? Percent { get; }
        
        /// <summary>
        /// Gets the timestamp when the job was created or started.
        /// </summary>
        public DateTime? Timestamp { get; }
        
        /// <summary>
        /// Gets the timestamp of the last status update.
        /// </summary>
        public DateTime? LastUpdated { get; }
        
        /// <summary>
        /// Gets the estimated time remaining for job completion.
        /// </summary>
        public TimeSpan? EstimatedTimeRemaining { get; }
        
        /// <summary>
        /// Gets the elapsed time since the job started.
        /// </summary>
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
