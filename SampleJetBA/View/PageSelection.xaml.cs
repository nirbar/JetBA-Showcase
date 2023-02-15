using PanelSW.Installer.JetBA.ViewModel;
using System.Windows.Controls;

namespace SampleJetBA.View
{
    public partial class PageSelectionView : UserControl
    {
        public PageSelectionView(NavigationViewModel nav, VariablesViewModelEx vars)
        {
            NavigationViewModel = nav;
            VariablesViewModel = vars;

            DataContext = this;
            InitializeComponent();
        }

        public NavigationViewModel NavigationViewModel { get; private set; }
        public VariablesViewModelEx VariablesViewModel { get; private set; }
    }
}