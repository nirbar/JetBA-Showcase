using PanelSW.Installer.JetBA.ViewModel;
using System.Windows.Controls;

namespace SampleJetBA.View
{
    public partial class PageSelectionView : UserControl
    {
        public PageSelectionView(NavigationViewModel nav, JetBundleVariables.BundleVariablesViewModel vars)
        {
            NavigationViewModel = nav;
            VariablesViewModel = vars;

            DataContext = this;
            InitializeComponent();
        }

        public NavigationViewModel NavigationViewModel { get; private set; }
        public JetBundleVariables.BundleVariablesViewModel VariablesViewModel { get; private set; }
    }
}