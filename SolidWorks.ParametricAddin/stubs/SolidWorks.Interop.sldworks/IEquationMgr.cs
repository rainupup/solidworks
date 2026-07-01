using System.Runtime.InteropServices;

namespace SolidWorks.Interop.sldworks
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IEquationMgr
    {
        int GetCount();
        string get_Equation(int Index);
        string get_Value(int Index);
        bool set_EquationAndConfiguration(int Index, string Equation, string Configuration, int Options);
        bool set_Value(int Index, string Value);
        bool UpdateAll();
    }
}
