using PanelSW.Installer.JetBA.ViewModel;
using SampleJetBA.ViewModel;
using System.Windows.Controls;

namespace SampleJetBA.View
{
    public partial class InstallLocationView : UserControl
    {
        public InstallLocationView(VariablesViewModelEx vars, NavigationViewModel nav)
        {
            NavigationViewModel = nav;
            VariablesViewModel = vars;

            DataContext = this;
            InitializeComponent();
        }

        public VariablesViewModelEx VariablesViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }
    }
}
