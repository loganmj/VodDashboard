using System.ComponentModel.DataAnnotations;

namespace VodDashboard.Api.Models
{
    public class PipelineSettings
    {
        [Required(AllowEmptyStrings = false)]
        public string InputDirectory { get; set; } = "";

        [Required(AllowEmptyStrings = false)]
        public string OutputDirectory { get; set; } = "";

        [Required(AllowEmptyStrings = false)]
        public string ConfigFile { get; set; } = "";
    }

}
