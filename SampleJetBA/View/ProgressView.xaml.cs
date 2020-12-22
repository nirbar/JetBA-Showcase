using PanelSW.Installer.JetBA.ViewModel;
using System.Windows.Controls;

namespace SampleJetBA.View
{
    public partial class ProgressView : UserControl
    {
        public ProgressView(NavigationViewModel nav, ApplyViewModel apply, ProgressViewModel prog)
        {
            DataContext = this;
            NavigationViewModel = nav;
            ApplyViewModel = apply;
            ProgressViewModel = prog;
            InitializeComponent();
        }

        public NavigationViewModel NavigationViewModel { get; private set; }
        public ApplyViewModel ApplyViewModel { get; private set; }
        public ProgressViewModel ProgressViewModel { get; private set; }
    }
}
