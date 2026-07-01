using System.Runtime.InteropServices;

namespace SolidWorks.Interop.sldworks
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IModelDoc2
    {
        object GetEquationMgr();
        string GetTitle();
        int GetType();
        string GetPathName();
        int EditRebuild3();
        bool Save3(int Options, ref int Errors, ref int Warnings);
        ModelDocExtension Extension { get; }
        int GetUnits(int UnitType);
    }

    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface ModelDocExtension
    {
        bool SaveAs(string FileName, int Version, int Options,
            object ExportData, ref int Errors, ref int Warnings);
        object get_CustomPropertyManager(string ConfigName);
        object GetPackAndGo();
    }
}
