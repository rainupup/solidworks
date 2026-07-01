using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using SolidWorks.ParametricAddin.Models;
using SolidWorks.ParametricAddin.TaskPane.ViewModels;

namespace SolidWorks.ParametricAddin.TaskPane
{
    [ComVisible(false)]
    public partial class UsageModeView : UserControl
    {
        public UsageModeView()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// Selects data template based on ParameterDefinition.ControlType and IsReadOnly.
    /// </summary>
    public class ParameterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextBoxTemplate { get; set; }
        public DataTemplate ComboBoxTemplate { get; set; }
        public DataTemplate ReadOnlyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ParameterInputViewModel paramInput)
            {
                var def = paramInput.Definition;

                // Read-only first
                if (def != null && def.IsReadOnly)
                    return ReadOnlyTemplate ?? TextBoxTemplate;

                // ComboBox when options are available
                if (def != null && def.ControlType == ControlType.ComboBox && def.Options?.Count > 0)
                    return ComboBoxTemplate ?? TextBoxTemplate;

                // Default: TextBox
                return TextBoxTemplate;
            }

            return TextBoxTemplate;
        }
    }
}
