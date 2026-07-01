using System;
using System.Collections.Generic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SolidWorks.ParametricAddin.Services
{
    public class EquationService
    {
        public List<(string Name, string Value, string FullEquation)> ReadAllEquations(IModelDoc2? modelDoc)
        {
            var result = new List<(string, string, string)>();
            var eqMgr = modelDoc?.GetEquationMgr() as IEquationMgr;
            if (eqMgr == null) return result;

            int count = eqMgr.GetCount();
            for (int i = 0; i < count; i++)
            {
                try
                {
                    string fullEquation = eqMgr.get_Equation(i);
                    string value = eqMgr.get_Value(i).ToString();
                    string name = ExtractVariableName(fullEquation);
                    if (!string.IsNullOrEmpty(name))
                        result.Add((name, value, fullEquation));
                }
                catch { }
            }
            return result;
        }

        public string ExtractVariableName(string fullEquation)
        {
            if (string.IsNullOrEmpty(fullEquation)) return string.Empty;
            int eqIndex = fullEquation.IndexOf('=');
            if (eqIndex <= 0) return string.Empty;
            return fullEquation.Substring(0, eqIndex).Trim().Trim('"').Trim();
        }

        public bool SetEquationValue(IModelDoc2? modelDoc, string equationName, string newValue)
        {
            var eqMgr = modelDoc?.GetEquationMgr() as IEquationMgr;
            if (eqMgr == null) return false;

            int count = eqMgr.GetCount();
            for (int i = 0; i < count; i++)
            {
                string fullEquation = eqMgr.get_Equation(i);
                string name = ExtractVariableName(fullEquation);
                if (string.Equals(name, equationName, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string newEquation = $"\"{name}\" = {newValue}";
                        eqMgr.set_Equation(i, newEquation);
                        return true;
                    }
                    catch { return false; }
                }
            }
            return false;
        }

        public bool SetEquationsBatch(IModelDoc2? modelDoc, Dictionary<string, string> equationValues)
        {
            var eqMgr = modelDoc?.GetEquationMgr() as IEquationMgr;
            if (eqMgr == null) return false;

            var indexMap = new Dictionary<int, string>();
            int count = eqMgr.GetCount();

            for (int i = 0; i < count; i++)
            {
                string name = ExtractVariableName(eqMgr.get_Equation(i));
                if (string.IsNullOrEmpty(name)) continue;
                foreach (var kvp in equationValues)
                {
                    if (string.Equals(name, kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        indexMap[i] = kvp.Value;
                        break;
                    }
                }
            }

            foreach (var kvp in indexMap)
            {
                string fullEquation = eqMgr.get_Equation(kvp.Key);
                string name = ExtractVariableName(fullEquation);
                eqMgr.set_Equation(kvp.Key, $"\"{name}\" = {kvp.Value}");
            }
            return true;
        }
    }
}
