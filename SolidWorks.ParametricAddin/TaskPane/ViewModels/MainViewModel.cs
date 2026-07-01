using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SolidWorks.Interop.sldworks;
using SolidWorks.ParametricAddin.Data;
using SolidWorks.ParametricAddin.Helpers;
using SolidWorks.ParametricAddin.Models;
using SolidWorks.ParametricAddin.Services;

namespace SolidWorks.ParametricAddin.TaskPane.ViewModels
{
    /// <summary>
    /// Main ViewModel. Owns the two modes and the mode-switching logic.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly ISldWorks _swApp;
        private readonly EquationService _equationService;
        private readonly ModelGeneratorService _modelGenerator;
        private readonly ValidationService _validationService;
        private readonly NamingService _namingService;
        private readonly RuleEngineService _ruleEngine;
        private readonly TemplateRepository _templateRepository;
        private readonly TemplateRegistry _templateRegistry;

        private DesignModeViewModel _designMode;
        private UsageModeViewModel _usageMode;
        private AppMode _currentMode;
        private string _statusMessage = "就绪";
        private bool _isBusy;

        public MainViewModel(ISldWorks swApp)
        {
            Logger.Info("MainViewModel constructor started");
            try
            {
                _swApp = swApp;
                _equationService = new EquationService();
                _validationService = new ValidationService();
                _namingService = new NamingService();
                _ruleEngine = new RuleEngineService();
                _modelGenerator = new ModelGeneratorService(swApp, _equationService, _namingService);

                string templatesDir = Helpers.SolidWorksHelper.GetTemplatesDir();
                _templateRepository = new TemplateRepository(templatesDir);
                _templateRegistry = new TemplateRegistry(templatesDir);

                _designMode = new DesignModeViewModel(swApp, _equationService, _templateRepository, _templateRegistry);
                _designMode.TemplateSaved += OnTemplateSaved;

                _usageMode = new UsageModeViewModel(
                    _modelGenerator, _validationService, _namingService,
                    _ruleEngine, _templateRepository, _templateRegistry);

                _currentMode = AppMode.ModeSelect;

                // Commands
                SwitchToDesignModeCommand = new RelayCommand(() => CurrentMode = AppMode.Design);
                SwitchToUsageModeCommand = new RelayCommand(() => CurrentMode = AppMode.Usage);
                SwitchToSelectModeCommand = new RelayCommand(() => CurrentMode = AppMode.ModeSelect);

                Logger.Info("MainViewModel constructor OK");
            }
            catch (Exception ex)
            {
                Logger.Error("MainViewModel constructor failed", ex);
                throw;
            }
        }

        public DesignModeViewModel DesignMode
        {
            get => _designMode;
            set => SetProperty(ref _designMode, value);
        }

        public UsageModeViewModel UsageMode
        {
            get => _usageMode;
            set => SetProperty(ref _usageMode, value);
        }

        public AppMode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (SetProperty(ref _currentMode, value))
                {
                    OnPropertyChanged(nameof(IsModeSelect));
                    OnPropertyChanged(nameof(IsDesignMode));
                    OnPropertyChanged(nameof(IsUsageMode));
                }
            }
        }

        public bool IsModeSelect => _currentMode == AppMode.ModeSelect;
        public bool IsDesignMode => _currentMode == AppMode.Design;
        public bool IsUsageMode => _currentMode == AppMode.Usage;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public RelayCommand SwitchToDesignModeCommand { get; }
        public RelayCommand SwitchToUsageModeCommand { get; }
        public RelayCommand SwitchToSelectModeCommand { get; }

        /// <summary>
        /// Updates the active document info.
        /// </summary>
        public void RefreshDocumentInfo()
        {
            IModelDoc2 activeDoc = _swApp?.IActiveDoc2;
            var docTitle = activeDoc?.GetTitle() ?? "无";
            StatusMessage = $"当前文档: {docTitle}";
        }

        private void OnTemplateSaved(TemplateConfig config)
        {
            // Load the saved template into usage mode for immediate testing
            _usageMode.LoadTemplate(config);
            StatusMessage = $"模板 '{config.TemplateName}' 已保存。可切换到使用模式测试。";
        }
    }

    public enum AppMode
    {
        ModeSelect,
        Design,
        Usage
    }
}
