using System;
using System.Collections.Generic;

namespace SolidWorks.ParametricAddin.Models
{
    /// <summary>
    /// Complete template configuration saved as JSON.
    /// </summary>
    public class TemplateConfig
    {
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateModelPath { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = string.Empty;
        public string NamingPattern { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>User-defined parameters mapped from SW equations.</summary>
        public List<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();

        /// <summary>Path to the product preview image shown in usage mode.</summary>
        public string PreviewImagePath { get; set; } = string.Empty;

        /// <summary>Visual rules for conditional parameter logic.</summary>
        public List<RuleDefinition> Rules { get; set; } = new List<RuleDefinition>();

        /// <summary>When the template was last modified.</summary>
        public DateTime LastModified { get; set; } = DateTime.Now;

        /// <summary>Template format version for forward compatibility.</summary>
        public int ConfigVersion { get; set; } = 1;
    }
}
