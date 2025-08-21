using System.Collections.Generic;

namespace App.Core.Configuration
{
    public class WktValidationConfig
    {
        public Dictionary<string, string> Patterns { get; set; } = new Dictionary<string, string>();
        public bool EnableRegexValidation { get; set; } = true;
        public bool EnableNetTopologyValidation { get; set; } = true;
    }
}