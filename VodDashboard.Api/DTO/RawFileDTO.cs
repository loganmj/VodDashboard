namespace VodDashboard.Api.DTO
{
    public record RawFileDTO
    {
        public string FileName { get; set; } = "";
        public long SizeBytes { get; set; }
        public DateTime Created { get; set; }
    }
}
