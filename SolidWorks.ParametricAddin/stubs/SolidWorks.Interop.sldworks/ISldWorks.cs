using System.Runtime.InteropServices;

namespace SolidWorks.Interop.sldworks
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface ISldWorks
    {
        IModelDoc2 IActiveDoc2 { get; }
        IModelDoc2 ActiveDoc { get; }
        bool SetAddinCallbackInfo2(int ModuleId, object Addin, int Cookie);
        bool AddMenuItem2(int DocumentType, int Cookie, string MenuItem, int Position,
            string MenuCallback, string MenuEnableMethod, string HintString, string StatusBarMethod);
        bool AddToolbarCommand2(int Cookie, string CommandName, int ToolbarId,
            int Position, string CallbackFunction, int ToolbarCommandType,
            string ToolTip);
        void RemoveMenu(string Name);
        bool RemoveToolbarCommand(int Cookie, string CommandName);
        IModelDoc2 IGetDocument(string FileName);
        bool OpenDoc6(string FileName, int Type, int Options, string Configuration,
            ref int Errors, ref int Warnings);
        object CreateTaskpaneView2(string ModulePath, string ControlName);
        void ShowTaskpaneView(object TaskpaneView);
        string GetVersion();
        int GetUnits(int UnitType);
    }
}
