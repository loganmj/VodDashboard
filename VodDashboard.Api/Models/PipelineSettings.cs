namespace VodDashboard.Api.Models
{
    public class PipelineSettings
    {
        public required string ConfigFile { get; set; }
        public string? FunctionStatusEndpoint { get; set; }
    }
}
