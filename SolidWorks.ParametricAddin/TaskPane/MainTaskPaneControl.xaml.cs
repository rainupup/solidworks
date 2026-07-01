using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using SolidWorks.Interop.sldworks;
using SolidWorks.ParametricAddin.TaskPane.ViewModels;

namespace SolidWorks.ParametricAddin.TaskPane
{
    [ComVisible(false)]
    public partial class MainTaskPaneControl : UserControl
    {
        public MainViewModel ViewModel => DataContext as MainViewModel;

        public MainTaskPaneControl()
        {
            InitializeComponent();
        }

        public void Initialize(ISldWorks swApp)
        {
            DataContext = new MainViewModel(swApp);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel?.RefreshDocumentInfo();
        }
    }
}
