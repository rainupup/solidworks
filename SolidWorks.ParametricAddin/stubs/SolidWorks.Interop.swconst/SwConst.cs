using System.Runtime.InteropServices;

namespace SolidWorks.Interop.swconst
{
    [ComVisible(true)]
    public enum swDocumentTypes_e
    {
        swDocNONE = 0,
        swDocPART = 1,
        swDocASSEMBLY = 2,
        swDocDRAWING = 3
    }

    public enum swOpenDocOptions_e
    {
        swOpenDocOptions_Silent = 1,
        swOpenDocOptions_ReadOnly = 2,
        swOpenDocOptions_LoadLightweight = 8
    }

    public enum swSaveAsVersion_e
    {
        swSaveAsCurrentVersion = 0
    }

    public enum swSaveAsOptions_e
    {
        swSaveAsOptions_Silent = 1,
        swSaveAsOptions_Copy = 2
    }

    public enum swLengthUnit_e
    {
        swMM = 0,
        swCM = 1,
        swMETER = 2,
        swINCHES = 3,
        swFEET = 4,
        swFEETINCHES = 5
    }

    public enum swCustomInfoType_e
    {
        swCustomInfoText = 30,
        swCustomInfoDate = 64,
        swCustomInfoNumber = 1,
        swCustomInfoYesOrNo = 17
    }

    public enum swToolbarCommandType_e
    {
        swToolbarCommandType_Standard = 1
    }
}
