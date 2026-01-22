namespace VodDashboard.Api.DTO
{
    public record PipelineConfig
    {
        public string InputDirectory { get; init; } = "";
        public string OutputDirectory { get; init; } = "";
        public string ArchiveDirectory { get; init; } = "";
        public bool EnableHighlights { get; init; }
        public bool EnableScenes { get; init; }
        public int SilenceThreshold { get; init; }
    }
}
