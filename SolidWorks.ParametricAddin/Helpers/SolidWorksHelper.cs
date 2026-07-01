using System;
using System.IO;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SolidWorks.ParametricAddin.Helpers
{
    /// <summary>
    /// Utility methods for common SolidWorks operations.
    /// </summary>
    public static class SolidWorksHelper
    {
        /// <summary>
        /// Gets the active document type as a display string.
        /// </summary>
        public static string GetDocumentTypeName(IModelDoc2 doc)
        {
            if (doc == null) return "无";

            return (swDocumentTypes_e)doc.GetType() switch
            {
                swDocumentTypes_e.swDocASSEMBLY => "装配体",
                swDocumentTypes_e.swDocPART => "零件",
                swDocumentTypes_e.swDocDRAWING => "工程图",
                _ => "未知"
            };
        }

        /// <summary>
        /// Gets the default templates directory for the add-in.
        /// </summary>
        public static string GetTemplatesDir()
        {
            string appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(appData, "SolidWorks", "ParametricAddin", "Templates");
            return dir;
        }

        /// <summary>
        /// Gets the default output directory for generated models.
        /// </summary>
        public static string GetDefaultOutputDir()
        {
            string docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            return Path.Combine(docs, "SolidWorks", "Generated");
        }
    }
}
