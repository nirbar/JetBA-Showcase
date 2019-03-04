using System.Windows.Controls;
using PanelSW.Installer.JetBA.JetPack.ViewModel;
using PanelSW.Installer.JetBA.ViewModel;

namespace SampleJetBA.View
{
    public partial class SummaryView : UserControl
    {
        public SummaryView(NavigationViewModel nav, ApplyViewModel apply, VariablesViewModel vars, PackagesViewModel pkgs)
        {
            ApplyViewModel = apply;
            NavigationViewModel = nav;
            VariablesViewModel = vars;
            PackagesViewModel = pkgs;

            DataContext = this;
            InitializeComponent();
        }

        public NavigationViewModel NavigationViewModel { get; private set; }
        public ApplyViewModel ApplyViewModel { get; private set; }
        public VariablesViewModel VariablesViewModel { get; private set; }
        public PackagesViewModel PackagesViewModel { get; private set; }
    }
}
