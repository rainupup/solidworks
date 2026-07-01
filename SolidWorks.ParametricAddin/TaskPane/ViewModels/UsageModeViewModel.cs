using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SolidWorks.ParametricAddin.Data;
using SolidWorks.ParametricAddin.Helpers;
using SolidWorks.ParametricAddin.Models;
using SolidWorks.ParametricAddin.Services;

namespace SolidWorks.ParametricAddin.TaskPane.ViewModels
{
    /// <summary>
    /// ViewModel for the Usage Mode page.
    /// </summary>
    public class UsageModeViewModel : BaseViewModel
    {
        private readonly ModelGeneratorService _modelGenerator;
        private readonly ValidationService _validationService;
        private readonly NamingService _namingService;
        private readonly RuleEngineService _ruleEngine;
        private readonly TemplateRepository _templateRepository;
        private readonly TemplateRegistry _templateRegistry;

        private TemplateConfig _loadedConfig;
        private ObservableCollection<ParameterInputViewModel> _parameterInputs =
            new ObservableCollection<ParameterInputViewModel>();
        private string _namingPreview = string.Empty;
        private string _statusMessage = "请加载一个模板。";
        private bool _isGenerating;
        private string _generationProgress;
        private string _currentTemplateName;
        private string _specNumber = string.Empty;
        private string _outputDirectory = string.Empty;
        private string _previewImagePath = string.Empty;

        public UsageModeViewModel(ModelGeneratorService modelGenerator, ValidationService validationService,
            NamingService namingService, RuleEngineService ruleEngine,
            TemplateRepository templateRepository, TemplateRegistry templateRegistry)
        {
            _modelGenerator = modelGenerator;
            _validationService = validationService;
            _namingService = namingService;
            _ruleEngine = ruleEngine;
            _templateRepository = templateRepository;
            _templateRegistry = templateRegistry;

            GenerateCommand = new RelayCommand(Generate, () => !_isGenerating);
            RefreshPreviewCommand = new RelayCommand(RefreshPreview);
            LoadTemplateByNameCommand = new RelayCommand(LoadTemplateByName);
            BrowseOutputCommand = new RelayCommand(BrowseOutput);
        }

        public TemplateConfig LoadedConfig
        {
            get => _loadedConfig;
            set => SetProperty(ref _loadedConfig, value);
        }

        public ObservableCollection<ParameterInputViewModel> ParameterInputs
        {
            get => _parameterInputs;
            set => SetProperty(ref _parameterInputs, value);
        }

        public string NamingPreview
        {
            get => _namingPreview;
            set => SetProperty(ref _namingPreview, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsGenerating
        {
            get => _isGenerating;
            set
            {
                if (SetProperty(ref _isGenerating, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string GenerationProgress
        {
            get => _generationProgress;
            set => SetProperty(ref _generationProgress, value);
        }

        public string CurrentTemplateName
        {
            get => _currentTemplateName;
            set => SetProperty(ref _currentTemplateName, value);
        }

        public string SpecNumber
        {
            get => _specNumber;
            set => SetProperty(ref _specNumber, value);
        }

        public string OutputDirectory
        {
            get => _outputDirectory;
            set => SetProperty(ref _outputDirectory, value);
        }

        public string PreviewImagePath
        {
            get => _previewImagePath;
            set => SetProperty(ref _previewImagePath, value);
        }

        public RelayCommand GenerateCommand { get; }
        public RelayCommand RefreshPreviewCommand { get; }
        public RelayCommand LoadTemplateByNameCommand { get; }
        public RelayCommand BrowseOutputCommand { get; }

        /// <summary>
        /// Load a template by its config object.
        /// </summary>
        public void LoadTemplate(TemplateConfig config)
        {
            Logger.Info($"LoadTemplate: {config.TemplateName}, params={config.Parameters.Count}");
            try
            {
                _loadedConfig = config;
                CurrentTemplateName = config.TemplateName;
                OutputDirectory = config.OutputDirectory;
                PreviewImagePath = config.PreviewImagePath;

                ParameterInputs.Clear();
                foreach (var param in config.Parameters)
                {
                    var input = new ParameterInputViewModel
                    {
                        Definition = param,
                        Value = param.DefaultValue,
                    };
                    input.ValueChanged += OnParameterValueChanged;
                    ParameterInputs.Add(input);
                }

                RefreshPreview();
                SpecNumber = NamingPreview;
                StatusMessage = $"已加载模板: {config.TemplateName} ({config.Parameters.Count} 个参数)";
                Logger.Info("LoadTemplate completed OK");
            }
            catch (Exception ex)
            {
                Logger.Error("LoadTemplate failed", ex);
                StatusMessage = $"加载模板失败: {ex.Message}";
            }
        }

        /// <summary>
        /// Open a file picker to select and load a template JSON file.
        /// </summary>
        public void LoadTemplateByName()
        {
            Logger.Info("LoadTemplateByName: opening file dialog");
            try
            {
                var owner = System.Windows.Application.Current?.Windows
                    .OfType<System.Windows.Window>().FirstOrDefault();
                Logger.Info($"Owner window: {(owner != null ? owner.Title : "null")}");

                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "选择参数化模板文件",
                    Filter = "JSON 模板文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                    DefaultExt = ".json",
                    InitialDirectory = System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                        "SolidWorks\\ParametricAddin\\Templates")
                };
                Logger.Info("File dialog created, calling ShowDialog");

                if (dialog.ShowDialog(owner) == true)
                {
                    Logger.Info($"File selected: {dialog.FileName}");
                    var config = _templateRepository.LoadFromPath(dialog.FileName);
                    if (config != null)
                    {
                        LoadTemplate(config);
                    }
                    else
                    {
                        StatusMessage = $"无法加载模板: {System.IO.Path.GetFileName(dialog.FileName)}";
                        Logger.Error("LoadFromPath returned null");
                    }
                }
                else
                {
                    Logger.Info("File dialog cancelled");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("LoadTemplateByName crashed", ex);
                StatusMessage = $"打开文件对话框失败: {ex.Message}";
            }
        }

        /// <summary>
        /// Load a template by name.
        /// </summary>
        public void LoadTemplate(string templateName)
        {
            var config = _templateRepository.Load(templateName);
            if (config != null)
            {
                LoadTemplate(config);
            }
            else
            {
                StatusMessage = $"未找到模板: '{templateName}'。";
            }
        }

        /// <summary>
        /// Load a template from a file path.
        /// </summary>
        public void LoadTemplateFromPath(string filePath)
        {
            var config = _templateRepository.LoadFromPath(filePath);
            if (config != null)
            {
                LoadTemplate(config);
            }
            else
            {
                StatusMessage = "无法加载模板文件。";
            }
        }

        private void OnParameterValueChanged()
        {
            RefreshPreview();
            SpecNumber = NamingPreview;
        }

        private void RefreshPreview()
        {
            if (_loadedConfig == null)
                return;

            var values = GetCurrentValues();
            NamingPreview = _namingService.Preview(_loadedConfig.NamingPattern, values);
        }

        private void BrowseOutput()
        {
            string selectedPath = ShowFolderBrowser("选择模型输出目录", OutputDirectory);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                OutputDirectory = selectedPath;
                if (_loadedConfig != null)
                    _loadedConfig.OutputDirectory = selectedPath;
            }
        }

        /// <summary>
        /// Shows a native folder browser dialog.
        /// </summary>
        private static string ShowFolderBrowser(string title, string initialPath)
        {
            var owner = System.Windows.Application.Current?.Windows
                .OfType<System.Windows.Window>().FirstOrDefault();

            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = title,
                Multiselect = false
            };
            if (!string.IsNullOrWhiteSpace(initialPath))
                dialog.FolderName = initialPath;

            if (dialog.ShowDialog(owner) == true)
                return dialog.FolderName;

            return null;
        }

        private Dictionary<string, string> GetCurrentValues()
        {
            var values = new Dictionary<string, string>();
            foreach (var input in ParameterInputs)
            {
                values[input.Definition.EquationName] = input.Value;
            }
            return values;
        }

        private void Generate()
        {
            if (_loadedConfig == null)
            {
                StatusMessage = "没有加载模板。";
                return;
            }

            // Step 1: Validate all user inputs
            var paramDefs = new Dictionary<string, ParameterDefinition>();
            var values = new Dictionary<string, string>();

            foreach (var input in ParameterInputs)
            {
                paramDefs[input.Definition.EquationName] = input.Definition;
                values[input.Definition.EquationName] = input.Value;
            }

            var errors = _validationService.ValidateAll(paramDefs, values);
            if (errors.Count > 0)
            {
                StatusMessage = string.Join("\n", errors);
                return;
            }

            // Step 2: Run rule engine to compute derived values
            if (_loadedConfig.Rules != null && _loadedConfig.Rules.Count > 0)
            {
                var computed = _ruleEngine.Evaluate(_loadedConfig.Rules, values);
                foreach (var kvp in computed)
                {
                    values[kvp.Key] = kvp.Value;
                }
            }

            // Step 3: Generate (use user-editable SpecNumber as output filename)
            IsGenerating = true;
            GenerationProgress = "生成中...";
            StatusMessage = "正在生成模型...";

            try
            {
                string outputFileName = !string.IsNullOrWhiteSpace(SpecNumber) ? SpecNumber : NamingPreview;
                var result = _modelGenerator.Generate(_loadedConfig, values, outputFileName);

                if (result.Success)
                {
                    GenerationProgress = "完成";
                    StatusMessage = $"模型已生成: {result.OutputPath}";
                    if (result.Warnings.Count > 0)
                    {
                        StatusMessage += $"\n警告: {string.Join("; ", result.Warnings)}";
                    }
                }
                else
                {
                    GenerationProgress = "失败";
                    StatusMessage = $"生成失败: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                GenerationProgress = "错误";
                StatusMessage = $"生成异常: {ex.Message}";
            }
            finally
            {
                IsGenerating = false;
            }
        }
    }

    /// <summary>
    /// ViewModel for a single parameter input field in Usage Mode.
    /// </summary>
    public class ParameterInputViewModel : BaseViewModel
    {
        private string _value;
        private string _validationError;

        public event Action ValueChanged;

        public ParameterDefinition Definition { get; set; }
        public string ValidationError
        {
            get => _validationError;
            set => SetProperty(ref _validationError, value);
        }

        public string Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                {
                    ValueChanged?.Invoke();
                }
            }
        }
    }
}
