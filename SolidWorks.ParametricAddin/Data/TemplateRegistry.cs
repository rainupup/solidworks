using System;
using System.IO;
using Newtonsoft.Json;

namespace SolidWorks.ParametricAddin.Data
{
    /// <summary>
    /// Maintains a list of all configured templates with metadata.
    /// Acts as the index for quick template lookup.
    /// </summary>
    public class TemplateRegistry
    {
        private readonly string _registryPath;

        public TemplateRegistry(string baseDirectory)
        {
            _registryPath = Path.Combine(baseDirectory, "template_registry.json");
        }

        public RegistryData Load()
        {
            if (!File.Exists(_registryPath))
                return new RegistryData();

            try
            {
                string json = File.ReadAllText(_registryPath);
                return JsonConvert.DeserializeObject<RegistryData>(json) ?? new RegistryData();
            }
            catch
            {
                return new RegistryData();
            }
        }

        public void Save(RegistryData data)
        {
            var dir = Path.GetDirectoryName(_registryPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(_registryPath, json);
        }

        public void AddOrUpdate(TemplateEntry entry)
        {
            var data = Load();
            var existing = data.Entries.Find(e =>
                string.Equals(e.TemplateName, entry.TemplateName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                data.Entries.Remove(existing);
            }

            data.Entries.Add(entry);
            Save(data);
        }

        public void Remove(string templateName)
        {
            var data = Load();
            data.Entries.RemoveAll(e =>
                string.Equals(e.TemplateName, templateName, StringComparison.OrdinalIgnoreCase));
            Save(data);
        }
    }

    public class RegistryData
    {
        public List<TemplateEntry> Entries { get; set; } = new List<TemplateEntry>();
    }

    public class TemplateEntry
    {
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateModelPath { get; set; } = string.Empty;
        public string ConfigFilePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
