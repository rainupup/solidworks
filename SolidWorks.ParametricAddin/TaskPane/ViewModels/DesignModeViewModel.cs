using System;
using System.Collections.ObjectModel;
using System.Linq;
using SolidWorks.Interop.sldworks;
using SolidWorks.ParametricAddin.Data;
using SolidWorks.ParametricAddin.Models;
using SolidWorks.ParametricAddin.Services;

namespace SolidWorks.ParametricAddin.TaskPane.ViewModels
{
    /// <summary>
    /// ViewModel for the Design Mode page.
    /// </summary>
    public class DesignModeViewModel : BaseViewModel
    {
        private readonly ISldWorks _swApp;
        private readonly EquationService _equationService;
        private readonly TemplateRepository _templateRepository;
        private readonly TemplateRegistry _templateRegistry;

        private TemplateConfig _currentConfig = new TemplateConfig();
        private ObservableCollection<ParameterDefinition> _parameters = new ObservableCollection<ParameterDefinition>();
        private ObservableCollection<RuleDefinition> _rules = new ObservableCollection<RuleDefinition>();
        private string _templateName = string.Empty;
        private string _outputDirectory = string.Empty;
        private string _namingPattern = string.Empty;
        private string _description = string.Empty;
        private ParameterDefinition _selectedParameter;
        private RuleDefinition _selectedRule;
        private string _statusMessage;

        public event Action<TemplateConfig> TemplateSaved;

        public DesignModeViewModel(ISldWorks swApp, EquationService equationService,
            TemplateRepository templateRepository, TemplateRegistry templateRegistry)
        {
            _swApp = swApp;
            _equationService = equationService;
            _templateRepository = templateRepository;
            _templateRegistry = templateRegistry;

            _outputDirectory = Helpers.SolidWorksHelper.GetDefaultOutputDir();

            // Commands
            ReadEquationsCommand = new RelayCommand(ReadEquationsFromSw);
            AddParameterCommand = new RelayCommand(AddParameter);
            RemoveParameterCommand = new RelayCommand(() => RemoveParameter(_selectedParameter));
            AddRuleCommand = new RelayCommand(AddRule);
            RemoveRuleCommand = new RelayCommand(() => RemoveRule(_selectedRule));
            SaveTemplateCommand = new RelayCommand(SaveTemplate);
            LoadTemplateCommand = new RelayCommand(LoadTemplate);
            ClearAllCommand = new RelayCommand(ClearAll);
        }

        public TemplateConfig CurrentConfig
        {
            get => _currentConfig;
            set => SetProperty(ref _currentConfig, value);
        }

        public ObservableCollection<ParameterDefinition> Parameters
        {
            get => _parameters;
            set => SetProperty(ref _parameters, value);
        }

        public ObservableCollection<RuleDefinition> Rules
        {
            get => _rules;
            set => SetProperty(ref _rules, value);
        }

        public string TemplateName
        {
            get => _templateName;
            set
            {
                if (SetProperty(ref _templateName, value))
                {
                    _currentConfig.TemplateName = value;
                    // Notify naming preview
                }
            }
        }

        public string OutputDirectory
        {
            get => _outputDirectory;
            set
            {
                if (SetProperty(ref _outputDirectory, value))
                    _currentConfig.OutputDirectory = value;
            }
        }

        public string NamingPattern
        {
            get => _namingPattern;
            set
            {
                if (SetProperty(ref _namingPattern, value))
                    _currentConfig.NamingPattern = value;
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                    _currentConfig.Description = value;
            }
        }

        public ParameterDefinition SelectedParameter
        {
            get => _selectedParameter;
            set => SetProperty(ref _selectedParameter, value);
        }

        public RuleDefinition SelectedRule
        {
            get => _selectedRule;
            set => SetProperty(ref _selectedRule, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand ReadEquationsCommand { get; }
        public RelayCommand AddParameterCommand { get; }
        public RelayCommand RemoveParameterCommand { get; }
        public RelayCommand AddRuleCommand { get; }
        public RelayCommand RemoveRuleCommand { get; }
        public RelayCommand SaveTemplateCommand { get; }
        public RelayCommand LoadTemplateCommand { get; }
        public RelayCommand ClearAllCommand { get; }

        private void ReadEquationsFromSw()
        {
            var modelDoc = _swApp?.IActiveDoc2;
            if (modelDoc == null)
            {
                StatusMessage = "请先打开一个 SolidWorks 模型。";
                return;
            }

            var equations = _equationService.ReadAllEquations(modelDoc);

            Parameters.Clear();

            foreach (var (name, value, fullEquation) in equations)
            {
                Parameters.Add(new ParameterDefinition
                {
                    EquationName = name,
                    DisplayName = name,
                    DefaultValue = value,
                    ControlType = ControlType.TextBox,
                    IsRequired = true
                });
            }

            // Set template model path from active document
            _currentConfig.TemplateModelPath = modelDoc.GetPathName();

            StatusMessage = $"从模型中读取到 {equations.Count} 个方程式/变量。";
        }

        private void AddParameter()
        {
            Parameters.Add(new ParameterDefinition
            {
                EquationName = "NewParam",
                DisplayName = "新参数",
                DefaultValue = "0",
                ControlType = ControlType.TextBox
            });

            StatusMessage = "已添加新参数，请修改其属性。";
        }

        private void RemoveParameter(ParameterDefinition param)
        {
            if (param != null && Parameters.Contains(param))
            {
                Parameters.Remove(param);
                StatusMessage = $"已删除参数 '{param.DisplayName}'。";
            }
        }

        private void AddRule()
        {
            Rules.Add(new RuleDefinition
            {
                Name = "新规则",
                Enabled = true
            });

            StatusMessage = "已添加新规则，请在规则编辑器中配置。";
        }

        private void RemoveRule(RuleDefinition rule)
        {
            if (rule != null && Rules.Contains(rule))
            {
                Rules.Remove(rule);
                StatusMessage = $"已删除规则 '{rule.Name}'。";
            }
        }

        private void SaveTemplate()
        {
            if (string.IsNullOrWhiteSpace(TemplateName))
            {
                StatusMessage = "请输入模板名称。";
                return;
            }

            var modelDoc = _swApp?.IActiveDoc2;
            if (modelDoc != null)
            {
                _currentConfig.TemplateModelPath = modelDoc.GetPathName();
            }

            _currentConfig.Parameters = Parameters.ToList();
            _currentConfig.Rules = Rules.ToList();

            string savedPath = _templateRepository.Save(_currentConfig);

            // Update registry
            _templateRegistry.AddOrUpdate(new TemplateEntry
            {
                TemplateName = _currentConfig.TemplateName,
                TemplateModelPath = _currentConfig.TemplateModelPath,
                ConfigFilePath = savedPath,
                Description = _currentConfig.Description,
                LastModified = DateTime.Now
            });

            StatusMessage = $"模板已保存: {savedPath}";
            TemplateSaved?.Invoke(_currentConfig);
        }

        private void LoadTemplate()
        {
            // Loading is handled by MainViewModel / file dialog
            // For now, reload from current config if available
            var templates = _templateRepository.ListTemplates();
            if (templates.Count > 0)
            {
                var config = _templateRepository.Load(templates.First());
                if (config != null)
                    PopulateFromConfig(config);
            }
        }

        public void PopulateFromConfig(TemplateConfig config)
        {
            _currentConfig = config;
            TemplateName = config.TemplateName;
            OutputDirectory = config.OutputDirectory;
            NamingPattern = config.NamingPattern;
            Description = config.Description;

            Parameters.Clear();
            foreach (var p in config.Parameters)
                Parameters.Add(p);

            Rules.Clear();
            foreach (var r in config.Rules)
                Rules.Add(r);

            StatusMessage = $"已加载模板 '{config.TemplateName}'。";
        }

        private void ClearAll()
        {
            _currentConfig = new TemplateConfig();
            Parameters.Clear();
            Rules.Clear();
            TemplateName = string.Empty;
            NamingPattern = string.Empty;
            Description = string.Empty;
            StatusMessage = "已清空所有内容。";
        }
    }
}
