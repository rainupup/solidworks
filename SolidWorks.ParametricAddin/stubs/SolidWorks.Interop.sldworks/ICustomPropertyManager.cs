using System.Runtime.InteropServices;

namespace SolidWorks.Interop.sldworks
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface ICustomPropertyManager
    {
        bool Add3(string Name, int Type, string Value);
        string Get2(string Name);
    }

    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IPackAndGo
    {
        object GetDocumentNames();
        bool SetDocumentName(string OldName, string NewName);
        bool IncludeDrawings { get; set; }
        bool IncludeSimulationResults { get; set; }
        bool FlattenToSingleFolder { get; set; }
    }
}
