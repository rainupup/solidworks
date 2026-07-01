using System.Collections.Generic;

namespace SolidWorks.ParametricAddin.Models
{
    /// <summary>
    /// Defines a user-facing parameter mapped to a SolidWorks equation/global variable.
    /// </summary>
    public class ParameterDefinition
    {
        /// <summary>Internal name matching the SW equation/variable name (e.g. "D1@Sketch1", "Length").</summary>
        public string EquationName { get; set; } = string.Empty;

        /// <summary>Human-readable label shown in the UI (e.g. "长度", "Material").</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Control type to render in the usage form.</summary>
        public ControlType ControlType { get; set; } = ControlType.TextBox;

        /// <summary>Default value pre-filled in the form.</summary>
        public string DefaultValue { get; set; } = string.Empty;

        /// <summary>Unit string (e.g. "mm", "inch", "deg").</summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>Category group for organizing parameters in the form.</summary>
        public string Group { get; set; } = "基本参数";

        /// <summary>Order within the group (lower = first).</summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>Whether the user must fill this parameter.</summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>Tooltip / help text.</summary>
        public string Description { get; set; } = string.Empty;

        // --- Constraints (applied based on ControlType) ---

        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double? Step { get; set; }
        public int? DecimalPlaces { get; set; }
        public string? RegexPattern { get; set; }
        public List<string> Options { get; set; } = new List<string>();

        /// <summary>When true, the user can view but not edit this parameter value.</summary>
        public bool IsReadOnly { get; set; } = false;
    }

    public enum ControlType
    {
        TextBox,
        NumericTextBox,
        ComboBox,
        CheckBox,
        Slider
    }
}
