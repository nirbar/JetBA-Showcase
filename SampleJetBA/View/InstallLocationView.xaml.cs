using PanelSW.Installer.JetBA.ViewModel;
using System.Windows.Controls;

namespace SampleJetBA.View
{
    public partial class InstallLocationView : UserControl
    {
        public InstallLocationView(JetBundleVariables.BundleVariablesViewModel vars, NavigationViewModel nav)
        {
            NavigationViewModel = nav;
            VariablesViewModel = vars;

            DataContext = this;
            InitializeComponent();
        }

        public JetBundleVariables.BundleVariablesViewModel VariablesViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }
    }
}
