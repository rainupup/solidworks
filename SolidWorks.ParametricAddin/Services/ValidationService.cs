using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SolidWorks.ParametricAddin.Models;

namespace SolidWorks.ParametricAddin.Services
{
    /// <summary>
    /// Validates user-supplied parameter values against their constraint definitions.
    /// </summary>
    public class ValidationService
    {
        /// <summary>
        /// Validates all parameter values. Returns a list of validation error messages.
        /// Empty list means all valid.
        /// </summary>
        public List<string> Validate(ParameterDefinition paramDef, string value)
        {
            var errors = new List<string>();

            // Required check
            if (paramDef.IsRequired && string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"'{paramDef.DisplayName}' 是必填参数。");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(value))
                return errors;

            // Control-type-specific validation
            switch (paramDef.ControlType)
            {
                case ControlType.NumericTextBox:
                case ControlType.Slider:
                    errors.AddRange(ValidateNumeric(paramDef, value));
                    break;

                case ControlType.ComboBox:
                    errors.AddRange(ValidateComboBox(paramDef, value));
                    break;

                case ControlType.TextBox:
                    errors.AddRange(ValidateTextBox(paramDef, value));
                    break;
            }

            return errors;
        }

        public List<string> ValidateAll(Dictionary<string, ParameterDefinition> paramDefs,
            Dictionary<string, string> values)
        {
            var allErrors = new List<string>();

            foreach (var kvp in paramDefs)
            {
                string paramName = kvp.Key;
                var def = kvp.Value;

                string value = values.ContainsKey(paramName) ? values[paramName] : string.Empty;
                var errors = Validate(def, value);

                allErrors.AddRange(errors.Select(e => $"[{def.DisplayName}] {e}"));
            }

            return allErrors;
        }

        private List<string> ValidateNumeric(ParameterDefinition def, string value)
        {
            var errors = new List<string>();

            if (!double.TryParse(value, out double numValue))
            {
                errors.Add($"'{value}' 不是有效的数值。");
                return errors;
            }

            if (def.MinValue.HasValue && numValue < def.MinValue.Value)
                errors.Add($"值不能小于 {def.MinValue.Value}。");

            if (def.MaxValue.HasValue && numValue > def.MaxValue.Value)
                errors.Add($"值不能大于 {def.MaxValue.Value}。");

            if (def.Step.HasValue && def.Step.Value > 0)
            {
                // Check step constraint (with tolerance)
                double remainder = (numValue - (def.MinValue ?? 0)) % def.Step.Value;
                if (Math.Abs(remainder) > 0.0001 && Math.Abs(remainder - def.Step.Value) > 0.0001)
                {
                    errors.Add($"值必须按步长 {def.Step.Value} 递增。");
                }
            }

            return errors;
        }

        private List<string> ValidateComboBox(ParameterDefinition def, string value)
        {
            var errors = new List<string>();

            if (def.Options.Count > 0 && !def.Options.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"'{value}' 不在可选范围内。可选项: {string.Join(", ", def.Options)}");
            }

            return errors;
        }

        private List<string> ValidateTextBox(ParameterDefinition def, string value)
        {
            var errors = new List<string>();

            if (!string.IsNullOrEmpty(def.RegexPattern))
            {
                try
                {
                    if (!Regex.IsMatch(value, def.RegexPattern))
                    {
                        errors.Add($"'{value}' 格式不正确。");
                    }
                }
                catch
                {
                    // Invalid regex pattern — skip validation
                }
            }

            return errors;
        }
    }
}
