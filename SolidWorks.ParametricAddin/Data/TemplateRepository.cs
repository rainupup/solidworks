using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SolidWorks.ParametricAddin.Models;

namespace SolidWorks.ParametricAddin.Data
{
    /// <summary>
    /// Loads and saves template configuration JSON files.
    /// </summary>
    public class TemplateRepository
    {
        private readonly string _templatesDirectory;

        public TemplateRepository(string templatesDirectory)
        {
            _templatesDirectory = templatesDirectory;
            if (!Directory.Exists(_templatesDirectory))
                Directory.CreateDirectory(_templatesDirectory);
        }

        /// <summary>
        /// Saves a template config to a JSON file.
        /// </summary>
        public string Save(TemplateConfig config)
        {
            config.LastModified = DateTime.Now;
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            string fileName = SanitizeFileName(config.TemplateName) + ".json";
            string filePath = Path.Combine(_templatesDirectory, fileName);

            File.WriteAllText(filePath, json);
            return filePath;
        }

        /// <summary>
        /// Loads a template config from a JSON file.
        /// </summary>
        public TemplateConfig? Load(string templateName)
        {
            string fileName = SanitizeFileName(templateName) + ".json";
            string filePath = Path.Combine(_templatesDirectory, fileName);

            if (!File.Exists(filePath))
                return null;

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<TemplateConfig>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Loads a template from an absolute file path.
        /// </summary>
        public TemplateConfig? LoadFromPath(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<TemplateConfig>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Lists all available template names.
        /// </summary>
        public List<string> ListTemplates()
        {
            var templates = new List<string>();

            if (!Directory.Exists(_templatesDirectory))
                return templates;

            foreach (string file in Directory.GetFiles(_templatesDirectory, "*.json"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                templates.Add(name);
            }

            return templates;
        }

        /// <summary>
        /// Deletes a template.
        /// </summary>
        public bool Delete(string templateName)
        {
            string fileName = SanitizeFileName(templateName) + ".json";
            string filePath = Path.Combine(_templatesDirectory, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Exports a template JSON to any path.
        /// </summary>
        public bool Export(TemplateConfig config, string exportPath)
        {
            try
            {
                config.LastModified = DateTime.Now;
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(exportPath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
