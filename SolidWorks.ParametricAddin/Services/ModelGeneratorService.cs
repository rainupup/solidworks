using System;
using System.Collections.Generic;
using System.IO;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.ParametricAddin.Models;

namespace SolidWorks.ParametricAddin.Services
{
    /// <summary>
    /// Orchestrates the model generation flow:
    /// Copy template -> replace equations -> rebuild -> save with new name.
    /// </summary>
    public class ModelGeneratorService
    {
        private readonly ISldWorks _swApp;
        private readonly EquationService _equationService;
        private readonly NamingService _namingService;

        public ModelGeneratorService(ISldWorks swApp, EquationService equationService, NamingService namingService)
        {
            _swApp = swApp;
            _equationService = equationService;
            _namingService = namingService;
        }

        /// <summary>
        /// Generates a new model from the template configuration and user-supplied parameter values.
        /// Returns the path to the generated model, or null on failure.
        /// </summary>
        public GenerationResult Generate(TemplateConfig config, Dictionary<string, string> parameterValues,
            string outputFileName = null)
        {
            var result = new GenerationResult();

            try
            {
                // Step 1: Validate input
                if (config == null || parameterValues == null || parameterValues.Count == 0)
                {
                    result.ErrorMessage = "Invalid configuration or parameter values.";
                    return result;
                }

                // Step 2: Ensure output directory exists
                string outputDir = !string.IsNullOrWhiteSpace(config.OutputDirectory)
                    ? config.OutputDirectory
                    : Path.GetDirectoryName(config.TemplateModelPath) ?? ".";
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Step 3: Compute the new filename
                string newFileName = !string.IsNullOrWhiteSpace(outputFileName)
                    ? _namingService.Resolve(outputFileName, parameterValues)
                    : _namingService.Resolve(config.NamingPattern, parameterValues);
                string fileExtension = Path.GetExtension(config.TemplateModelPath);
                string newFilePath = Path.Combine(outputDir, newFileName + fileExtension);

                // Handle naming conflicts
                newFilePath = ResolveNamingConflict(newFilePath);

                // Step 4: Open template model
                IModelDoc2 templateDoc = OpenTemplateModel(config.TemplateModelPath);
                if (templateDoc == null)
                {
                    result.ErrorMessage = $"Failed to open template model: {config.TemplateModelPath}";
                    return result;
                }

                // Step 5: Save a copy using SaveAs (for parts) or Pack and Go (for assemblies)
                bool copySuccess = SaveAsCopy(templateDoc, newFilePath);
                if (!copySuccess)
                {
                    result.ErrorMessage = "Failed to copy template model.";
                    return result;
                }

                // Step 6: Open the newly created model
                IModelDoc2 newDoc = OpenTemplateModel(newFilePath);
                if (newDoc == null)
                {
                    result.ErrorMessage = "Failed to open newly created model.";
                    return result;
                }

                // Step 7: Replace equation values
                bool equationsUpdated = _equationService.SetEquationsBatch(newDoc, parameterValues);
                if (!equationsUpdated)
                {
                    result.Warnings.Add("Some equations could not be updated.");
                }

                // Step 8: Force rebuild
                newDoc.EditRebuild3();

                // Step 9: Update custom properties (metadata)
                UpdateCustomProperties(newDoc, config, parameterValues);

                // Step 10: Save
                int saveErrors = 0, saveWarnings = 0;
                newDoc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref saveErrors, ref saveWarnings);

                result.Success = true;
                result.OutputPath = newFilePath;
                result.Warnings.AddRange(new[] { $"Save errors: {saveErrors}, warnings: {saveWarnings}" });
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Generation failed: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Opens a SolidWorks document, returning the model doc.
        /// Returns null if the document is already open (returns existing instance).
        /// </summary>
        private IModelDoc2 OpenTemplateModel(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            // Check if already open
            IModelDoc2 existingDoc = _swApp.GetOpenDocumentByName(Path.GetFileName(filePath)) as IModelDoc2;
            if (existingDoc != null)
                return existingDoc;

            int docType = DetermineDocType(filePath);
            int errors = 0, warnings = 0;

            _swApp.OpenDoc6(
                filePath,
                docType,
                (int)swOpenDocOptions_e.swOpenDocOptions_Silent,
                "",
                ref errors,
                ref warnings
            );

            return _swApp.IActiveDoc2;
        }

        private bool SaveAsCopy(IModelDoc2 modelDoc, string targetPath)
        {
            try
            {
                int errors = 0, warnings = 0;
                int saveAsVersion = (int)swSaveAsVersion_e.swSaveAsCurrentVersion;
                int options = (int)swSaveAsOptions_e.swSaveAsOptions_Silent |
                              (int)swSaveAsOptions_e.swSaveAsOptions_Copy;

                modelDoc.Extension.SaveAs(
                    targetPath,
                    saveAsVersion,
                    options,
                    null,
                    ref errors,
                    ref warnings
                );

                return errors == 0;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateCustomProperties(IModelDoc2 modelDoc, TemplateConfig config,
            Dictionary<string, string> parameterValues)
        {
            try
            {
                var customPropMgr = modelDoc.Extension.get_CustomPropertyManager("") as ICustomPropertyManager;
                if (customPropMgr == null)
                    return;

                customPropMgr.Add3("ParametricTemplate", (int)swCustomInfoType_e.swCustomInfoText, config.TemplateName, 1);
                customPropMgr.Add3("GeneratedFrom", (int)swCustomInfoType_e.swCustomInfoText, config.TemplateModelPath, 1);
                customPropMgr.Add3("GeneratedDate", (int)swCustomInfoType_e.swCustomInfoText,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 1);

                foreach (var kvp in parameterValues)
                {
                    customPropMgr.Add3($"Param_{kvp.Key}", (int)swCustomInfoType_e.swCustomInfoText, kvp.Value, 1);
                }
            }
            catch
            {
                // Custom properties are non-critical
            }
        }

        private string ResolveNamingConflict(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            string dir = Path.GetDirectoryName(filePath) ?? "";
            string nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            string ext = Path.GetExtension(filePath);

            int suffix = 1;
            string newPath;
            do
            {
                newPath = Path.Combine(dir, $"{nameWithoutExt}_{suffix}{ext}");
                suffix++;
            }
            while (File.Exists(newPath) && suffix < 1000);

            return newPath;
        }

        private int DetermineDocType(string filePath)
        {
            string ext = Path.GetExtension(filePath)?.ToUpperInvariant();
            return ext switch
            {
                ".SLDPRT" => (int)swDocumentTypes_e.swDocPART,
                ".SLDASM" => (int)swDocumentTypes_e.swDocASSEMBLY,
                ".SLDDRW" => (int)swDocumentTypes_e.swDocDRAWING,
                _ => (int)swDocumentTypes_e.swDocPART
            };
        }
    }

    public class GenerationResult
    {
        public bool Success { get; set; }
        public string OutputPath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
