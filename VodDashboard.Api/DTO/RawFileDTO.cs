namespace VodDashboard.Api.DTO
{
    public class RawFileDTO
    {
        public string FileName { get; set; } = "";
        public long SizeBytes { get; set; }
        public DateTime Created { get; set; }
    }
}
