using System.Collections.Generic;

namespace SolidWorks.ParametricAddin.Models
{
    /// <summary>
    /// A single IF-THEN-ELSE rule built visually in design mode.
    /// </summary>
    public class RuleDefinition
    {
        public string Name { get; set; } = string.Empty;
        public List<RuleCondition> Conditions { get; set; } = new List<RuleCondition>();
        public LogicOperator Logic { get; set; } = LogicOperator.AND;
        public List<RuleAction> ThenActions { get; set; } = new List<RuleAction>();
        public List<RuleAction> ElseActions { get; set; } = new List<RuleAction>();
        public bool Enabled { get; set; } = true;
    }

    public class RuleCondition
    {
        /// <summary>Parameter name to evaluate (refers to ParameterDefinition.EquationName).</summary>
        public string Param { get; set; } = string.Empty;

        public ComparisonOperator Op { get; set; } = ComparisonOperator.Equal;

        /// <summary>The value to compare against (string form, cast as needed).</summary>
        public string Value { get; set; } = string.Empty;
    }

    public class RuleAction
    {
        /// <summary>Target parameter name.</summary>
        public string Param { get; set; } = string.Empty;

        /// <summary>Value to assign (string form, cast as needed).</summary>
        public string Value { get; set; } = string.Empty;
    }

    public enum ComparisonOperator
    {
        GreaterThan,
        LessThan,
        GreaterOrEqual,
        LessOrEqual,
        Equal,
        NotEqual,
        Contains
    }

    public enum LogicOperator
    {
        AND,
        OR
    }
}
