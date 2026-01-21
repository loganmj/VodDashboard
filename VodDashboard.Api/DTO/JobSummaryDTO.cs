namespace VodDashboard.Api.DTO
{
    public record JobSummaryDTO
    {
        public required string Id { get; set; }
        public bool HasCleanVideo { get; set; }
        public int HighlightCount { get; set; }
        public int SceneCount { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
