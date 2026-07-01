using System.Runtime.InteropServices;

namespace SolidWorks.Interop.swpublished
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface ISwAddin
    {
        bool ConnectToSW(object ThisSW, int Cookie);
        bool DisconnectFromSW();
    }
}
