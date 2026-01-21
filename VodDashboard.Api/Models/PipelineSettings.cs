namespace VodDashboard.Api.Models
{
    public class PipelineSettings
    {
        public required string InputDirectory { get; set; }
        public required string OutputDirectory { get; set; }
        public required string ConfigFile { get; set; }
    }

}
