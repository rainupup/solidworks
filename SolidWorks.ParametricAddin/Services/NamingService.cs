using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SolidWorks.ParametricAddin.Services
{
    /// <summary>
    /// Resolves naming templates like "传送带_L{Length}_W{Width}" to actual filenames.
    /// </summary>
    public class NamingService
    {
        private static readonly Regex TemplateRegex = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        /// <summary>
        /// Replaces {ParamName} placeholders with actual parameter values.
        /// </summary>
        public string Resolve(string namingPattern, Dictionary<string, string> parameterValues)
        {
            if (string.IsNullOrEmpty(namingPattern))
                return "GeneratedModel";

            string result = TemplateRegex.Replace(namingPattern, match =>
            {
                string paramName = match.Groups[1].Value;
                if (parameterValues.TryGetValue(paramName, out string paramValue))
                {
                    return SanitizeSegment(paramValue);
                }

                return match.Value; // Leave unmatched placeholders
            });

            return SanitizeWholePath(result);
        }

        /// <summary>
        /// Preview the resolved name without sanitization (for display).
        /// </summary>
        public string Preview(string namingPattern, Dictionary<string, string> parameterValues)
        {
            if (string.IsNullOrEmpty(namingPattern))
                return "GeneratedModel";

            return TemplateRegex.Replace(namingPattern, match =>
            {
                string paramName = match.Groups[1].Value;
                if (parameterValues.TryGetValue(paramName, out string paramValue))
                    return paramValue;
                return match.Value;
            });
        }

        /// <summary>
        /// Extract parameter names used in the naming template.
        /// </summary>
        public List<string> GetReferencedParameters(string namingPattern)
        {
            var names = new List<string>();
            if (string.IsNullOrEmpty(namingPattern))
                return names;

            foreach (Match match in TemplateRegex.Matches(namingPattern))
            {
                string name = match.Groups[1].Value;
                if (!names.Contains(name))
                    names.Add(name);
            }

            return names;
        }

        private string SanitizeSegment(string input)
        {
            foreach (char c in InvalidFileNameChars)
            {
                input = input.Replace(c, '_');
            }
            return input;
        }

        private string SanitizeWholePath(string path)
        {
            // Handle directory separators in naming pattern (allows subdirectories)
            string[] segments = path.Split('/', '\\');

            for (int i = 0; i < segments.Length; i++)
            {
                foreach (char c in InvalidFileNameChars)
                {
                    segments[i] = segments[i].Replace(c, '_');
                }
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(), segments);
        }
    }
}
