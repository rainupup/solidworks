using System;
using System.Collections.Generic;
using SolidWorks.ParametricAddin.Models;

namespace SolidWorks.ParametricAddin.Services
{
    /// <summary>
    /// Evaluates visual rules defined in design mode.
    /// Rules are evaluated in order; each rule's THEN/ELSE actions
    /// assign computed values to parameters.
    /// </summary>
    public class RuleEngineService
    {
        /// <summary>
        /// Evaluates all enabled rules against the current parameter values.
        /// Returns a dictionary of computed parameter values (overrides).
        /// </summary>
        public Dictionary<string, string> Evaluate(List<RuleDefinition> rules,
            Dictionary<string, string> currentValues)
        {
            var computed = new Dictionary<string, string>();

            foreach (var rule in rules)
            {
                if (!rule.Enabled)
                    continue;

                if (rule.Conditions.Count == 0)
                    continue;

                bool conditionMet = EvaluateConditions(rule.Conditions, rule.Logic, currentValues);

                var actions = conditionMet ? rule.ThenActions : rule.ElseActions;

                foreach (var action in actions)
                {
                    computed[action.Param] = action.Value;
                }
            }

            return computed;
        }

        private bool EvaluateConditions(List<RuleCondition> conditions, LogicOperator logic,
            Dictionary<string, string> values)
        {
            bool result = logic == LogicOperator.AND;

            foreach (var condition in conditions)
            {
                bool single = EvaluateSingleCondition(condition, values);

                if (logic == LogicOperator.AND)
                {
                    result = result && single;
                    if (!result) break; // Short-circuit
                }
                else // OR
                {
                    result = result || single;
                    if (result) break; // Short-circuit
                }
            }

            return result;
        }

        private bool EvaluateSingleCondition(RuleCondition condition, Dictionary<string, string> values)
        {
            if (!values.TryGetValue(condition.Param, out string actualValue))
                return false;

            // Try numeric comparison first
            if (double.TryParse(actualValue, out double numActual) &&
                double.TryParse(condition.Value, out double numCompare))
            {
                return condition.Op switch
                {
                    ComparisonOperator.GreaterThan => numActual > numCompare,
                    ComparisonOperator.LessThan => numActual < numCompare,
                    ComparisonOperator.GreaterOrEqual => numActual >= numCompare,
                    ComparisonOperator.LessOrEqual => numActual <= numCompare,
                    ComparisonOperator.Equal => Math.Abs(numActual - numCompare) < 0.0001,
                    ComparisonOperator.NotEqual => Math.Abs(numActual - numCompare) > 0.0001,
                    ComparisonOperator.Contains => actualValue.Contains(condition.Value,
                        StringComparison.OrdinalIgnoreCase),
                    _ => false
                };
            }

            // Fall back to string comparison
            return condition.Op switch
            {
                ComparisonOperator.Equal => string.Equals(actualValue, condition.Value,
                    StringComparison.OrdinalIgnoreCase),
                ComparisonOperator.NotEqual => !string.Equals(actualValue, condition.Value,
                    StringComparison.OrdinalIgnoreCase),
                ComparisonOperator.Contains => actualValue.Contains(condition.Value,
                    StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }

        /// <summary>
        /// Validates rule syntax. Returns list of issues.
        /// </summary>
        public List<string> ValidateRule(RuleDefinition rule, List<ParameterDefinition> availableParams)
        {
            var issues = new List<string>();
            var paramNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in availableParams)
                paramNames.Add(p.EquationName);

            foreach (var c in rule.Conditions)
            {
                if (!paramNames.Contains(c.Param))
                    issues.Add($"规则 '{rule.Name}': 条件参数 '{c.Param}' 未定义。");
            }

            foreach (var a in rule.ThenActions)
            {
                if (!paramNames.Contains(a.Param))
                    issues.Add($"规则 '{rule.Name}': THEN 动作参数 '{a.Param}' 未定义。");
            }

            foreach (var a in rule.ElseActions)
            {
                if (!paramNames.Contains(a.Param))
                    issues.Add($"规则 '{rule.Name}': ELSE 动作参数 '{a.Param}' 未定义。");
            }

            return issues;
        }
    }
}
